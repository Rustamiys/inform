using System.IO;
using System.Threading.Tasks;
using System;

namespace appManager
{
    internal class ArtifactManager
    {
        private ProcessManegering processManager = new ProcessManegering();
        private FileManagering fileManager = new FileManagering();
        internal Task taskKill(string path, TimeSpan waitingtTime, int processWaitingTime)
        {
            return processManager.errorHandler(path, "./kill.ps1", waitingtTime, processWaitingTime);
        }
        internal Task backupAndRestore(string path, TimeSpan waitingtTime, int processWaitingTime)
        {
            return processManager.errorHandler(path, "./backup-and-restore.cmd", waitingtTime, processWaitingTime);
        }
        internal Task updateDB(string path, string dbname, TimeSpan waitingtTime, int processWaitingTime)
        {
            return processManager.errorHandler(path, $"./update-db.cmd {dbname} -w", waitingtTime, processWaitingTime);
        }
        internal Task removeBinObj(string path, TimeSpan waitingtTime, int processWaitingTime)
        {
            return processManager.errorHandler(path, "./remove-bin-obj.ps1", waitingtTime, processWaitingTime);
        }
        internal Task removeNuget(string[] cache)
        {           
            return Task.Factory.StartNew(() =>
            {
                foreach (string path in cache)
                {
                    DirectoryInfo folder = new DirectoryInfo(path);
                    fileManager.cleanFiles(folder);
                }
            });
        }
//        internal Task restoreNuget(string EXEpath, string path, TimeSpan waitingtTime, int processWaitingTime)
        internal Task restoreNuget(string EXEpath, TimeSpan waitingtTime, int processWaitingTime)

        {
            return processManager.errorHandler(EXEpath, "./nuget restore", waitingtTime, processWaitingTime);
            //return processManager.errorHandler(EXEpath, $"set NUGET_PACKAGES={path}; ./nuget restore", waitingtTime, processWaitingTime);

        }
        internal Task removeNodeModules(string path)
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            return Task.Factory.StartNew(() => fileManager.cleanFiles(folder));
        }
        internal Task removePackageLock(string path)
        {
            return Task.Factory.StartNew(() => fileManager.cleanFiles(path));
        }
        internal Task npmRestore(string path, TimeSpan waitingtTime, int processWaitingTime)
        {
            return processManager.errorHandler(path, "npm i", waitingtTime, processWaitingTime);
        }
    }
}
