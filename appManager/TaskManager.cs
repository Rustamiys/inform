using System.IO;
using System.Threading.Tasks;
using System;
using System.Xml.Linq;

namespace appManager
{
    internal class TaskManager
    {
        private ProcessManager processManager = new ProcessManager();
        private FileManager fileManager = new FileManager();
        internal void taskKill(string path)
        {
            processManager.runProcess(path, "powershell.exe", "./kill.ps1");
        }
        internal void backupAndRestore(string path)
        {
            processManager.tryRunProcess(path, "backup-and-restore.cmd", "");
        }
        internal void updateDB(string path, string dbname)
        {
            processManager.runProcess(path, "update-db.cmd", $"{dbname} -w");
        }
        internal void removeBinObj(string path)
        {
            processManager.runProcess(path, "powershell.exe", "./remove-bin-obj.ps1");
        }
        internal void removeNuget(string[] cache)
        {                       
            foreach (string path in cache)
            {
                var folder = new DirectoryInfo(path);
                fileManager.cleanFiles(folder);
            };
        }
        internal void restoreNuget(string path)
        {
            processManager.runProcess(path, "nuget", "restore");
        }
        internal void removeNodeModules(string path)
        {
            var folder = new DirectoryInfo(path);
            fileManager.cleanFiles(folder);
        }
        internal void removePackageLock(string path)
        {
            fileManager.cleanFiles(path);
        }
        internal void npmRestore(string path)
        {
            processManager.runProcess(path, "npm", "i");
        }
    }
}
