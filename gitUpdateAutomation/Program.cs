using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Runtime.InteropServices;

namespace gitUpdateAutomation
{
    internal class Program
    {
        class LocalData
        {
            public string informIusPath;
            public string nginxConfPath;
            public string DB_BackupPath;
            public Param param;
            public bool? removeBinObj;
            public bool? removeNuget;
            public bool? removeNodeModules;
            public bool? removePackageLock;
            public bool? recoveryNugetPackage;
            public bool? recoveryNpmPackage;
            public string[] cachePath;
            public string nugetExePath;
            public TimeSpan processWaitingTime;
        }
        class Param
        {
            public string PARAM_PORT_PREFIX;
            public string PARAM_DIST_PATH;
            public string PARAM_DATABASE;
            public string PARAM_VHOST;
        }

        private static void setValues(string name, ref bool? param)
        {
            while (true)
            {
                Console.Write($"Use {name} (y/n): ");
                string ch = Convert.ToString(Console.ReadLine());
                if (ch == "y") { param = true; return; }
                if (ch == "n") { param = false; return; }
                Console.WriteLine("wrong input");
            }
        }

        private static void replaceParam(string path, Param param)
        {
            StreamReader reader = new StreamReader(path);
            string content = reader.ReadToEnd();
            reader.Close();

            content = content.Replace("%PARAM_PORT_PREFIX%", param.PARAM_PORT_PREFIX);
            content = content.Replace("%PARAM_DIST_PATH%", param.PARAM_DIST_PATH);
            content = content.Replace("%PARAM_DATABASE%", param.PARAM_DATABASE);
            content = content.Replace("%PARAM_VHOST%", param.PARAM_VHOST);

            StreamWriter writer = new StreamWriter(path);
            writer.Write(content);
            writer.Close();
        }
        private static void configurationReplacement(string path, string pathMove, string filename, Param param)
        {
            path += "/" + filename;
            pathMove += "/" + filename;
            FileInfo fileInf = new FileInfo(path);
            if (fileInf.Exists)
            {
                fileInf.CopyTo(pathMove, true);
                replaceParam(pathMove, param);
            }
            else
            {
                Console.WriteLine($"wrong path: {path}");
            }
        }     
        private static async Task errorHandler(string path, string command, TimeSpan timespan)
        {
            try
            {
                await runProcessWithTimeoutAsync(path, command, timespan);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        private static async Task runProcessWithTimeoutAsync(string path, string command, TimeSpan timespan)
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(timespan);
                try
                {
                    await Task.Run(() => runProcess(path, command, cts.Token), cts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException($"Время выполнения команды '{command}' истекло.");
                }
            }
        }

        private static void runProcess(string path, string command, CancellationToken token)
        {
            command = $"-NoExit -Command cd {path};{command}";
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = true,
                CreateNoWindow = false,
                WorkingDirectory = path,
                Arguments = command
            };

            Process process = new Process();
            process.StartInfo = processInfo;
            process.Start();
            while (true)
            {
                if (process.HasExited)
                {
                    return;
                }
                else if (token.IsCancellationRequested)
                {
                    process.Kill();
                    throw new OperationCanceledException(token);
                }
            }
        }
        static async Task Main()
        {
            string localConfPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Git Update Auto/localConf.json";
            if (!File.Exists(localConfPath))
            {
                Console.WriteLine($"No such file or directory {localConfPath}");
                return;
            }    
            string jsonString = File.ReadAllText(localConfPath);
            LocalData localData = JsonConvert.DeserializeObject<LocalData>(jsonString);

            Console.Write($"Copy sorce database name: ");
            string dbname = Convert.ToString(Console.ReadLine());

            if (!localData.removeBinObj.HasValue) { setValues("remove-bin-obj", ref localData.removeBinObj); }
            if (!localData.removeNuget.HasValue) { setValues("remove nuget cache", ref localData.removeNuget); }
            if (!localData.removeNodeModules.HasValue) { setValues("remove node-modules", ref localData.removeNodeModules); }
            if (!localData.removePackageLock.HasValue) { setValues("remove package-lock", ref localData.removePackageLock); }
            if (!localData.recoveryNpmPackage.HasValue) { setValues("recovery npm package", ref localData.recoveryNpmPackage); }
            if (!localData.recoveryNugetPackage.HasValue) { setValues("recovery nuget pacckage", ref localData.recoveryNugetPackage); }

            // Копирование конфигурационных файлов WebApi на уровень выше и замена параметров
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.nginxConfPath, "nginx.conf", localData.param);
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.informIusPath + "/WebApi", "ServiceConfiguration.json", localData.param);
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.nginxConfPath, "upstreams.conf", localData.param);
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.nginxConfPath, "proxy-services.conf", localData.param);

            // Копирование экземпляров WebClient на уровень выше и замена параметров
            configurationReplacement(localData.informIusPath + "/WebClient/teamplates", localData.informIusPath + "/WebClient", ".env.dev.local", localData.param);
            configurationReplacement(localData.informIusPath + "/WebClient/teamplates", localData.informIusPath + "/WebClient", ".env.local", localData.param);



            Task task_kill = Task.Factory.StartNew(() => {
                Console.WriteLine("Start process kill.ps1");
                errorHandler(localData.informIusPath + "/WebApi", "./kill.ps1", localData.processWaitingTime).Wait();
                Console.WriteLine("Finish process kill.ps1");
            });
            await task_kill;
            Task task_backup_and_restore = Task.Factory.StartNew(() => {
                Console.WriteLine("Start process backup-and-restore.cmd");
                errorHandler(localData.DB_BackupPath, "./backup-and-restore.cmd", localData.processWaitingTime).Wait();
                Console.WriteLine("Finish process backup-and-restore.cmd");
                Console.WriteLine("Start process update-db.cmd");
                errorHandler(localData.informIusPath, $"./update-db.cmd {dbname} -w", localData.processWaitingTime).Wait();
                Console.WriteLine("Finish process update-db.cmd");
            });

            Task task_remove_bin_obj = Task.Factory.StartNew(() => {
                if (localData.removeBinObj.Value)
                {
                    Console.WriteLine("Start process remove-bin-obj.ps1");
                    errorHandler(localData.informIusPath, "./remove-bin-obj.ps1", localData.processWaitingTime).Wait();
                    Console.WriteLine("Finish process remove-bin-obj.ps1");
                }
            });

            Task task_remove_nuget = Task.Factory.StartNew(() => {
                if (localData.removeNuget.Value)
                {
                    Console.WriteLine("Start process remove nuget");
                    foreach (string path in localData.cachePath)
                    {
                        DirectoryInfo folder = new DirectoryInfo(path);
                        foreach (FileInfo file in folder.GetFiles())
                        {
                            file.Delete();
                        }

                        foreach (DirectoryInfo dir in folder.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                    }
                    Console.WriteLine("Finish process remove nuget");
                }
                if (localData.recoveryNugetPackage.Value)
                {
                    Console.WriteLine("Start process restore nuget");
//                    errorHandler(localData.nugetExePath, $"./set NUGET_PACKAGES={localData.cachePath[1]};./nuget restore", localData.processWaitingTime).Wait();
                    errorHandler(localData.nugetExePath, $"set NUGET_PACKAGES={localData.cachePath[1]}; ./nuget restore", localData.processWaitingTime).Wait();
                    Console.WriteLine("Finish process restore nuget");
                }
            });
            Task task_remove_node_modules = Task.Factory.StartNew(() =>
            {
                if (localData.removeNodeModules.Value)
                {
                    Console.WriteLine("Start process remove node modules");
                    DirectoryInfo folder = new DirectoryInfo(localData.informIusPath + "/WebApi/node_modules");
                    if (folder.Exists)
                    {
                        foreach (FileInfo file in folder.GetFiles())
                        {
                            file.Delete();
                        }

                        foreach (DirectoryInfo dir in folder.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No such file or directory {folder.FullName}");
                    }
                    Console.WriteLine("Finish process remove node modules");
                }
                if (localData.removePackageLock.Value)
                {
                    string packageLockpath = localData.informIusPath + "/WebApi/package-lock.json";
                    if (File.Exists(packageLockpath))
                    {
                        File.Delete(packageLockpath);
                    }
                    else
                    {
                        Console.WriteLine($"No such file or directory {packageLockpath}");
                    }
                }
                if (localData.recoveryNpmPackage.Value)
                {
                    Console.WriteLine("Start process npm i");
                    errorHandler(localData.informIusPath + "/WebApi", "npm i", localData.processWaitingTime).Wait();
                    Console.WriteLine("Finish process npm i");
                }
            });
            Task.WaitAll(task_backup_and_restore, task_remove_bin_obj, task_remove_nuget, task_remove_node_modules);
        }
    }
}
