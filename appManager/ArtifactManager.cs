using System.IO;
using System.Threading.Tasks;
using System;

namespace appManager
{
    internal class ArtifactManager
    {
        public static Task taskKill(string path, TimeSpan waitingtTime)
        {
            return ProcessManegering.errorHandler(path, "./kill.ps1", waitingtTime);
        }
        public static Task backupAndRestore(string path, TimeSpan waitingtTime)
        {
            return ProcessManegering.errorHandler(path, "./backup-and-restore.cmd", waitingtTime);
        }
        public static Task updateDB(string path, string dbname, TimeSpan waitingtTime)
        {
            return ProcessManegering.errorHandler(path, $"./update-db.cmd {dbname} -w", waitingtTime);
        }
        public static Task removeBinObj(string path, TimeSpan waitingtTime)
        {
            return ProcessManegering.errorHandler(path, "./remove-bin-obj.ps1", waitingtTime);
        }
        public static Task removeNuget(string[] cache)
        {           
            return Task.Factory.StartNew(() =>
            {
                foreach (string path in cache)
                {
                    DirectoryInfo folder = new DirectoryInfo(path);
                    FileManagering.cleanFiles(folder);
                }
            });
        }
        public static Task restoreNuget(string EXEpath, string path, TimeSpan waitingtTime)
        {
            return ProcessManegering.errorHandler(EXEpath, $"set NUGET_PACKAGES={path}; ./nuget restore", waitingtTime);

        }
        public static Task removeNodeModules(string path)
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            return Task.Factory.StartNew(() => FileManagering.cleanFiles(folder));
        }
        public static Task removePackageLock(string path)
        {
            return Task.Factory.StartNew(() => FileManagering.cleanFiles(path));
        }
        public static Task npmRestore(string path, TimeSpan waitingtTime)
        {
            return ProcessManegering.errorHandler(path, "npm i", waitingtTime);
        }
    }
}
