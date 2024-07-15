using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

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
            public string removeBinObj;
            public string removeNuget;
            public string removeNodeModules;
            public string removePackageLock;
            public string recoveryNugetPackage;
            public string recoveryNpmPackage;
            public string[] cachePath;
            public string nugetExePath;
        }
        class Param
        {
            public string PARAM_PORT_PREFIX;
            public string PARAM_DIST_PATH;
            public string PARAM_DATABASE;
            public string PARAM_VHOST;
        }
        private static void setValues(string name, ref string param)
        {
            while (true)
            {
                Console.Write($"Use {name} (y/n): ");
                string ch = Convert.ToString(Console.ReadLine());
                if (ch == "y") { param = "true"; return; }
                if (ch == "n") { param = "false"; return; }
                Console.WriteLine("wrong input");
            }
        }

        private static void replaceParam(string path, Param param)
        {
            StreamReader reader = new StreamReader(path);
            string content = reader.ReadToEnd();
            reader.Close();

            content = Regex.Replace(content, "%PARAM_PORT_PREFIX%", param.PARAM_PORT_PREFIX);
            content = Regex.Replace(content, "%PARAM_DIST_PATH%", param.PARAM_DIST_PATH);
            content = Regex.Replace(content, "%PARAM_DATABASE%", param.PARAM_DATABASE);
            content = Regex.Replace(content, "%PARAM_VHOST%", param.PARAM_VHOST);

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
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                WorkingDirectory = path,
                Arguments = command
            };

            Process process = new Process();
            process.StartInfo = processInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (token.IsCancellationRequested)
            {
                process.Close();
                throw new OperationCanceledException(token);
            }
            process.Close();
            Console.Write("Output:");
            Console.WriteLine(output);
            Console.Write("Error:");
            Console.WriteLine(error);
        }
        static void Main()
        {
            string localConfPath = "C:/Users/admin/Documents/inf/localConf.json";
            if (!File.Exists(localConfPath))
            {
                Console.WriteLine($"No such file or directory {localConfPath}");
                return;
            }
            string jsonString = File.ReadAllText(localConfPath);
            LocalData localData = JsonConvert.DeserializeObject<LocalData>(jsonString);

            if (localData.removeBinObj == null) { setValues("remove-bin-obj", ref localData.removeBinObj); }
            if (localData.removeNuget == null) { setValues("remove nuget cache", ref localData.removeNuget); }
            if (localData.removeNodeModules == null) { setValues("remove node-modules", ref localData.removeNodeModules); }
            if (localData.removePackageLock == null) { setValues("remove package-lock", ref localData.removePackageLock); }
            if (localData.recoveryNpmPackage == null) { setValues("recovery npm package", ref localData.recoveryNpmPackage); }
            if (localData.recoveryNugetPackage == null) { setValues("recovery nuget pacckage", ref localData.recoveryNugetPackage); }

            // Копирование конфигурационных файлов WebApi на уровень выше и замена параметров
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.nginxConfPath, "nginx.conf", localData.param);
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.informIusPath + "/WebApi", "ServiceConfiguration.json", localData.param);
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.nginxConfPath, "upstreams.conf", localData.param);
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.nginxConfPath, "proxy-services.conf", localData.param);

            // Копирование экземпляров WebClient на уровень выше и замена параметров
            configurationReplacement(localData.informIusPath + "/WebClient/teamplates", localData.informIusPath + "/WebClient", ".env.dev.local", localData.param);
            configurationReplacement(localData.informIusPath + "/WebClient/teamplates", localData.informIusPath + "/WebClient", ".env.local", localData.param);

            TimeSpan timeSpan = TimeSpan.FromSeconds(5);

            Task task_kill = new Task(() => {
                Console.WriteLine("Start process kill.ps1");
                errorHandler(localData.informIusPath + "/WebApi", "/c powershell -file kill.ps1", timeSpan).Wait();
                Console.WriteLine("Finish process kill.ps1");
            });
            Task task_backup_and_restore = new Task(() => {
                Console.WriteLine("Start process backup-and-restore.cmd");
                errorHandler(localData.DB_BackupPath, "/c backup-and-restore.cmd", timeSpan).Wait();
                Console.WriteLine("Finish process backup-and-restore.cmd");
            });
            Task task_update_db = task_backup_and_restore.ContinueWith(async t => {
                Console.WriteLine("Start process update-db.cmd");
                errorHandler(localData.informIusPath, $"/c update-db.cmd {localData.param.PARAM_DATABASE} -w", timeSpan).Wait();
                Console.WriteLine("Finish process update-db.cmd");
            });
            Task task_remove_bin_obj = new Task(() => {
                if (localData.removeBinObj == "true")
                {
                    Console.WriteLine("Start process remove-bin-obj.ps1");
                    errorHandler(localData.informIusPath, "/c powershell -file remove-bin-obj.ps1", timeSpan).Wait();
                    Console.WriteLine("Finish process remove-bin-obj.ps1");
                }
            });
            Task task_remove_nuget = new Task(() => {
                if (localData.removeNuget == "true")
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
            });
            Task task_restore_nuget = task_remove_nuget.ContinueWith(t =>
            {
                if (localData.recoveryNugetPackage == "true")
                {
                    Console.WriteLine("Start process restore nuget");
                    errorHandler(localData.nugetExePath, $"/c set NUGET_PACKAGES={localData.cachePath[1]} & nuget restore", timeSpan).Wait();
                    Console.WriteLine("Finish process restore nuget");
                }
            });
            Task task_remove_node_modules = new Task(() =>
            {
                if (localData.removeNodeModules == "true")
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
                if (localData.removePackageLock == "true")
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
                if (localData.recoveryNpmPackage == "true")
                {
                    Console.WriteLine("Start process npm i");
                    errorHandler(localData.informIusPath + "/WebApi", "/c npm i", timeSpan).Wait();
                    Console.WriteLine("Finish process npm i");
                }
            });
            task_kill.Start();
            task_kill.Wait();
            task_backup_and_restore.Start();
            task_remove_bin_obj.Start();
            task_remove_nuget.Start();
            task_remove_node_modules.Start();
            Task.WaitAll(task_backup_and_restore, task_update_db, task_remove_bin_obj,
                task_remove_nuget, task_restore_nuget, task_remove_node_modules);
        }
    }
}
