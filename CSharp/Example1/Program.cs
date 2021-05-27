using System;
using System.Threading.Tasks;
using QuantForce;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Example1
{
    class Program
    {
        static async Task MainAsync(string[] args)
        {
            try
            {
                // HAve a look a the Wiki for API documentation
                SuperSimpleParser.CommandLineParser clp = SuperSimpleParser.CommandLineParser.Parse(Environment.CommandLine);
                // Get user name and password from environnement variables
                string userName = clp.GetString("QFUser", Environment.GetEnvironmentVariable("QFUser"));
                string password = clp.GetString("QFPassword", Environment.GetEnvironmentVariable("QFPassword"));
                string endPoint = clp.GetString("QFEndpoint", Environment.GetEnvironmentVariable("QFEndpoint"));
                if (string.IsNullOrEmpty(endPoint))
                    endPoint = "https://portal.quantforce.net"; // Default endpoint
                string apiEndPoint = endPoint.AppendToURL("api", "v1.0");

                Rest client = new Rest();

                // Authentication
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                string md5password = Convert.ToBase64String(md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(password)));
                var auth = await client.PostAsync<AuthResult>(apiEndPoint.AppendToURL("auth"), new Auth()
                {
                    authType = QuantForce.Type.MD5,
                    login = userName,
                    param1 = md5password
                });

                // Search or create the project
                string projectName = "demo01";
                var projects = await client.GetAsync<ProjectList>(apiEndPoint.AppendToURL("project", auth.token));
                var project = projects.projects.Find(_ => _.name == projectName);
                if (project == null)
                {
                    Console.WriteLine("Creating a new project");
                    // Doesn't exist, create it
                    project = await client.PostAsync<Project>(apiEndPoint.AppendToURL("project", auth.token), new Project()
                    {
                        name = projectName,
                        type = 0,
                        subType = 0
                    });
                }
                else
                    Console.WriteLine("Use existing project.");

                if (project != null)
                {
                    Console.WriteLine("Project");
                    Console.WriteLine(JsonConvert.SerializeObject(project));
                    apiEndPoint = project.uri.AppendToURL("api", "v1.0");

                    // Upload the dataset
                    string data = "Telco_customer_churn_v1.csv";
                    var task = await client.PostRawAsync<AsyncTaskStatus>(apiEndPoint.AppendToURL("dataset", auth.token, project.id, "csv", "raw", "65001"), System.IO.File.ReadAllBytes(data)); // 65001 = UTF-8
                    if (task == null)
                        return;

                    // Wait for the dataset to be integrated
                    while ((int)task.status < 400)
                    {
                        Console.WriteLine("Dataset task status = {0}", task.status);
                        await Task.Delay(1000);
                        task = await client.GetAsync<AsyncTaskStatus>(apiEndPoint.AppendToURL("task", auth.token, project.id, task.id));
                    }
                    Console.WriteLine("Dataset task status = {0}", task.status);

                    // Get dataset infos
                    var dataset = await client.GetAsync<Dataset>(apiEndPoint.AppendToURL("dataset", auth.token, project.id));
                    if (dataset == null)
                        return;
                    // Force the target
                    foreach (Column column in dataset.columns)
                    {
                        if (column.columType == VariableType.Target)
                            column.columType = VariableType.Ignore;
                        if (column.name == "Churn_Value")
                            column.columType = VariableType.Target;
                        Console.WriteLine($"Column {column.name} is {column.columType}.");
                    }
                    // Update column qualifications
                    await client.PostAsync<Dataset>(apiEndPoint.AppendToURL("dataset", auth.token, project.id), dataset);

                    // Compute binning for all column
                    task = await client.GetAsync<AsyncTaskStatus>(apiEndPoint.AppendToURL("binning", "create", auth.token, project.id, "*", "20"));
                    if (task == null)
                        return;

                    // Wait for the binning computation
                    while ((int)task.status < 400)
                    {
                        Console.WriteLine("Binning task status = {0}", task.status);
                        await Task.Delay(1000);
                        task = await client.GetAsync<AsyncTaskStatus>(apiEndPoint.AppendToURL("task", auth.token, project.id, task.id));
                    }
                    Console.WriteLine("Binning task status = {0}", task.status);

                    BinsViewList binning = await client.GetAsync<BinsViewList>(apiEndPoint.AppendToURL("binning", "get", auth.token, project.id, "*", "Auto"));
                    if (binning == null)
                        return;
                    foreach (BinsView bv in binning.all)
                    {
                        // Display the binning
                        Console.WriteLine(JsonConvert.SerializeObject(bv));
                    }

                    // download Python code
                    await client.DownloadAsync(apiEndPoint.AppendToURL("deploy", "export", auth.token, project.id, "Python"), "transform.py");
                    // download Excel
                    await client.DownloadAsync(apiEndPoint.AppendToURL("deploy", "export", auth.token, project.id, "Excel"), "transform.xlsx");

                    // Let the api do the tranformation
                    task = await client.PostRawAsync<AsyncTaskStatus>(apiEndPoint.AppendToURL("deploy", auth.token, project.id, "csv", "raw", "65001"), System.IO.File.ReadAllBytes(data)); // 65001 = UTF-8
                    if (task == null)
                        return;

                    // Wait for the dataset to be integrated
                    while ((int)task.status < 400)
                    {
                        Console.WriteLine("Dataset task status = {0}", task.status);
                        await Task.Delay(1000);
                        task = await client.GetAsync<AsyncTaskStatus>(apiEndPoint.AppendToURL("task", auth.token, project.id, task.id));
                    }
                    Console.WriteLine("Dataset task status = {0}", task.status);
                    // Get the transformed dataset
                    await client.DownloadAsync(apiEndPoint.AppendToURL("deploy", "export", auth.token, project.id, "transform"), "data_t.csv");
                    Console.WriteLine("Transformed data downloaded.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        static void Main(string[] args)
        {
            Task.WaitAll(Task.Run(async () => await MainAsync(args)));
            Console.ReadLine();
        }
    }
}
