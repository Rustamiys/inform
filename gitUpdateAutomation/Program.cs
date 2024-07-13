using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading;


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

        private static void runProcess(string path, string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "cmd.exe";
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = false;
            processInfo.WorkingDirectory = path;
            processInfo.Arguments = command;

            Process process = new Process();
            process.StartInfo = processInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

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
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.nginxConfPath,"nginx.conf", localData.param);
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.informIusPath + "/WebApi","ServiceConfiguration.json", localData.param);
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.nginxConfPath, "upstreams.conf", localData.param);
            configurationReplacement(localData.informIusPath + "/WebApi/configuration", localData.nginxConfPath, "proxy-services.conf", localData.param);

            // Копирование экземпляров WebClient на уровень выше и замена параметров
            configurationReplacement(localData.informIusPath + "/WebClient/teamplates", localData.informIusPath + "/WebClient", ".env.dev.local", localData.param);
            configurationReplacement(localData.informIusPath + "/WebClient/teamplates", localData.informIusPath + "/WebClient", ".env.local", localData.param);

            // Запуск процессов    
            runProcess(localData.informIusPath + "/WebApi", "/c powershell -file kill.ps1");
            runProcess(localData.DB_BackupPath, "/c backup-and-restore.cmd");
            runProcess(localData.informIusPath, $"/c update-db.cmd {localData.param.PARAM_DATABASE} -w");
            if (localData.recoveryNpmPackage == "true")
            {

                runProcess(localData.informIusPath + "/WebApi", "/c npm i");
            }
            if (localData.recoveryNugetPackage == "true")
            {
                runProcess(localData.nugetExePath, $"/c set NUGET_PACKAGES={localData.cachePath[1]} & nuget restore");
            }
            if (localData.removeBinObj == "true" )
            {
                runProcess(localData.informIusPath, "/c powershell -file remove-bin-obj.ps1");
            }

            // Удаление кэша
            if (localData.removeNuget == "true")
            {
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
            }

            // Удаление node-modules
            if (localData.removeNodeModules == "true")
            {
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
            }

            // Удаление package-lock
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
        }
    }
}
