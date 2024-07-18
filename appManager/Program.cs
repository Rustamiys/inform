using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace appManager
{
    public class Program
    {
        private static void Main(string[] args)
        {

        }
        public static void runUtility() {
            Console.Write($"Copy sorce database name: ");
            string dbname = Convert.ToString(Console.ReadLine());

            string localConfPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Git Update Auto/localConf.json";

            LocalConfiguration config = FileManagering.getConfig(localConfPath);

            config.setValuesIfNull();
            FileManagering.copyAndReplaceConfig(config);

            Task task_kill = Task.Factory.StartNew(() => {
                ProcessManegering.errorHandler(config.informIusPath + "/WebApi", "./kill.ps1", config.processWaitingTime).Wait();
            });
            task_kill.Wait();
            Task task_backup_and_restore = Task.Factory.StartNew(() => {
                ProcessManegering.errorHandler(config.DB_BackupPath, "./backup-and-restore.cmd", config.processWaitingTime).Wait();
                ProcessManegering.errorHandler(config.informIusPath, $"./update-db.cmd {dbname} -w", config.processWaitingTime).Wait();
            });

            Task task_remove_bin_obj = Task.Factory.StartNew(() => {
                if (config.removeBinObj.Value)
                {
                    ProcessManegering.errorHandler(config.informIusPath, "./remove-bin-obj.ps1", config.processWaitingTime).Wait();
                }
            });

            Task task_remove_nuget = Task.Factory.StartNew(() => {
                if (config.removeNuget.Value)
                {
                    foreach (string path in config.cachePath)
                    {
                        DirectoryInfo folder = new DirectoryInfo(path);
                        FileManagering.cleanFiles(folder);
                    }
                    Console.WriteLine("All nuget cache removed");
                }
                if (config.recoveryNugetPackage.Value)
                {
                    ProcessManegering.errorHandler(config.nugetExePath, $"set NUGET_PACKAGES={config.cachePath[1]}; ./nuget restore", config.processWaitingTime).Wait();
                }
            });
            Task task_remove_node_modules = Task.Factory.StartNew(() =>
            {
                if (config.removeNodeModules.Value)
                {
                    DirectoryInfo folder = new DirectoryInfo(config.informIusPath + "/WebApi/node_modules");
                    FileManagering.cleanFiles(folder);
                    Console.WriteLine("Node modules removed");
                }
                if (config.removePackageLock.Value)
                {
                    string packageLockpath = config.informIusPath + "/WebApi/package-lock.json";
                    FileManagering.cleanFiles(packageLockpath);
                    Console.WriteLine("Package-lock removed");
                }
                if (config.recoveryNpmPackage.Value)
                {
                    ProcessManegering.errorHandler(config.informIusPath + "/WebApi", "npm i", config.processWaitingTime).Wait();
                }
            });
            Task.WaitAll(task_backup_and_restore, task_remove_bin_obj, task_remove_nuget, task_remove_node_modules);
        }
    }
}
