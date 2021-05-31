import os

from utils.folder_file_manager import make_directory_if_not_exists

CUR_DIR = os.path.dirname(os.path.abspath(__file__))
OUTPUT_DIR = make_directory_if_not_exists(os.path.join(CUR_DIR, 'output'))

TASK_STATUS = {0: "wait", 100: "toProcess", 200: "stating", 300: "running", 400: "done", 401: "error",
               402: "stopped", 500: "killed"}
COLUMN_TYPE = {1: "Undefined", 2: "Ignore", 3: "Continue", 4: "Nominal", 5: "Target", 6: "Id", 7: "Weight"}

DATA_FILE_PATH = "..\Telco_customer_churn_v1.csv"
