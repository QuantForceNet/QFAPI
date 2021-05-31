import time
import os
import hashlib
import base64
import json
import argparse
import requests
import urllib3

from utils.folder_file_manager import save_file
from settings import DATA_FILE_PATH, TASK_STATUS, COLUMN_TYPE, OUTPUT_DIR


class QuantForceAPI:
    def __init__(self):
        self.session = requests.Session()
        self.session.headers = {
            "Content-Type": "application/json;charset=utf-8",
            "Accept": "application/json;charset=utf-8"
        }
        self.parser = argparse.ArgumentParser()
        self.parser.add_argument("-u", "--user", action="store", required=False, dest="user", help="Your Email", default = os.environ.get("QFUser"))
        self.parser.add_argument("-p", "--password", action="store", required=False, dest="password", help="Your Password", default = os.environ.get("QFPassword"))
        self.parser.add_argument("-pn", "--project", action="store", required=False, dest="project_name", help="Project Name", default="demo01")
        self.parser.add_argument("-ep", "--endpoint", action="store", required=False, dest="endpoint", help="API Endpoint", default=os.environ.get("QFEndpoint", default="http://portal.quantforce.net"))
        urllib3.disable_warnings()

    def manage_task(self, endpoint, path, task, token, project_id, data=None):
        if data is not None:
            task_res = self.session.post(url=endpoint + path, data=data, verify=False)
        else:
            task_res = self.session.get(url=endpoint + path, verify=False)
        current_task = json.loads(task_res.text)
        if current_task is None:
            return

        # Wait for the dataset to be integrated
        while current_task["status"] < 400:
            print(f"[INFO] {task} task status = {TASK_STATUS[current_task['status']]}")
            time.sleep(1)
            task_res = self.session.get(endpoint + f"/task/{token}/{project_id}/{current_task['id']}", verify=False)
            current_task = json.loads(task_res.text)
        print(f"[INFO] {task} task status = {TASK_STATUS[current_task['status']]}")

        return

    def run(self):
        try:
            # Have a look a the Wiki for API documentation

            # Get user name and password, project name from environment variables
            args = self.parser.parse_args()
            user = args.user
            password = args.password
            project_name = args.project_name
            endpoint = args.endpoint + "/api/v1"

            # Authentication
            hash_pwd = base64.b64encode(hashlib.md5(password.encode('ascii')).digest()).decode('ascii')
            auth_res = self.session.post(url=endpoint + "/auth", json={"authType": 1, "login": user, "param1": hash_pwd}, verify=False)
            token = json.loads(auth_res.text)["token"]

            # Search or Create project
            projects_res = self.session.get(url=endpoint + "/project" + f"/{token}", verify=False)
            quant_projects = json.loads(projects_res.text)["projects"]
            current_project = {}
            for q_project in quant_projects:
                if q_project["name"] == project_name:
                    current_project = q_project
            if current_project == {}:
                print(f"[INFO] Creating a new project...")
                # Doesn't exist, create it
                new_project_res = self.session.post(endpoint + "/project" + f"/{token}", json={"name": project_name, "type": 0, "subType": 0}, verify=False)
                current_project = json.loads(new_project_res.text)
            else:
                print(f"[INFO] Use existing project.")

            if current_project is not None:
                print(f"[INFO] Project:\n{current_project}")
                endpoint = current_project["uri"] + "/api/v1"
                # Upload the dataset
                in_file = open(DATA_FILE_PATH, "rb")
                data = in_file.read()
                in_file.close()
                self.manage_task(endpoint, f"/dataset/{token}/{current_project['id']}/csv/raw/65001", task="Dataset", data=data, token=token, project_id=current_project['id'])

                # Get dataset info
                dataset_res = self.session.get(endpoint + f"/dataset/{token}/{current_project['id']}", verify=False)
                dataset = json.loads(dataset_res.text)
                if dataset is None:
                    return

                # Force the target
                for column in dataset["columns"]:
                    if column["columType"] == 5:
                        column["columType"] = 2
                    if column["name"] == "Churn_Value":
                        column["columType"] = 5
                    print(f"[INFO] Column {column['name']} is {COLUMN_TYPE[column['columType']]}")

                # Update column qualifications
                self.session.post(url=endpoint + f"/dataset/{token}/{current_project['id']}", json=dataset, verify=False)

                # Computing binining for all column
                self.manage_task(endpoint, f"/binning/create/{token}/{current_project['id']}/*/20", task="Binning", token=token, project_id=current_project['id'])

                binning_res = self.session.get(endpoint + f"/binning/get/{token}/{current_project['id']}/*/Auto", verify=False)
                binning = json.loads(binning_res.text)
                if binning is None:
                    return
                for bin_element in binning["all"]:
                    # Display the binning
                    print(f"[INFO] {bin_element}")

                # Download Python Code
                download_python_res = self.session.get(endpoint + f"/deploy/export/{token}/{current_project['id']}/Python", verify=False)
                save_file(content=download_python_res.content, filename=os.path.join(OUTPUT_DIR, 'transform.py'))

                # Download Excel
                download_excel_res = self.session.get(endpoint + f"/deploy/export/{token}/{current_project['id']}/Excel", verify=False)
                save_file(content=download_excel_res.content, filename=os.path.join(OUTPUT_DIR, 'transform.xlsx'))

                # Let the api do the transformation
                self.manage_task(endpoint, f"/deploy/{token}/{current_project['id']}/csv/raw/65001", task="Dataset", token=token, project_id=current_project['id'], data=data)

                # Get the transformed dataset
                download_csv_res = self.session.get(endpoint + f"/deploy/export/{token}/{current_project['id']}/transform", verify=False)
                save_file(content=download_csv_res.content, filename=os.path.join(OUTPUT_DIR, 'data_t.csv'))
                print("[INFO] Transformed data downloaded")

        except Exception as e:
            print(f"[WARN] {e}")

        return


if __name__ == '__main__':
    QuantForceAPI().run()
