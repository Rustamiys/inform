using System.IO;
using System;

namespace appManager
{
    /// <summary>
    /// Класс менеджера задач, отвечающий за их вызов
    /// </summary>
    internal class TaskManager
    {
        private ProcessManager _processManager = new ProcessManager();
        private FileManager _fileManager = new FileManager();
        /// <summary>
        /// задача выполнения kill.ps1
        /// </summary>
        /// <param name="path"></param>
        /// <param name="waitingTime"></param>
        internal void taskKill(string path, TimeSpan waitingTime)
        {
            _processManager.runProcess(path, "powershell.exe", "./kill.ps1", waitingTime);
        }
        /// <summary>
        /// задача выполнения резервного копирования и восстановления backup-and-restore.cmd
        /// </summary>
        /// <param name="path"></param>
        /// <param name="waitingTime"></param>
        internal void backupAndRestore(string path, TimeSpan waitingTime)
        {
            _processManager.tryRunProcess(path, "backup-and-restore.cmd", "", waitingTime);
        }
        /// <summary>
        /// задача выполнения обновления базы данных update-db.cmd $"{dbname} -w
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dbname"></param>
        /// <param name="waitingTime"></param>
        internal void updateDB(string path, string dbname, TimeSpan waitingTime)
        {
            _processManager.runProcess(path, "update-db.cmd", $"{dbname} -w", waitingTime);
        }
        /// <summary>
        /// задача выполнения удаления бинарных файлов remove-bin-obj.ps1
        /// </summary>
        /// <param name="path"></param>
        /// <param name="waitingTime"></param>
        internal void removeBinObj(string path, TimeSpan waitingTime)
        {
            _processManager.runProcess(path, "powershell.exe", "./remove-bin-obj.ps1", waitingTime);
        }
        /// <summary>
        /// задача удаления кэша
        /// </summary>
        /// <param name="cache"></param>
        internal void removeNuget(string[] cache)
        {                       
            foreach (string path in cache)
            {
                var folder = new DirectoryInfo(path);
                _fileManager.cleanFiles(folder);
            };
        }
        /// <summary>
        /// задача восстановления nuget пакетов
        /// </summary>
        /// <param name="path"></param>
        /// <param name="waitingTime"></param>
        internal void restoreNuget(string path, TimeSpan waitingTime)
        {
            _processManager.runProcess(path, "nuget", "restore", waitingTime);
        }
        /// <summary>
        /// задача удаления node_modules
        /// </summary>
        /// <param name="path"></param>
        internal void removeNodeModules(string path)
        {
            var folder = new DirectoryInfo(path);
            _fileManager.cleanFiles(folder);
        }
        /// <summary>
        /// задача удаления package-lock.json 
        /// </summary>
        /// <param name="path"></param>
        internal void removePackageLock(string path)
        {
            _fileManager.cleanFiles(path);
        }
        /// <summary>
        /// задача восстановления node_modules
        /// </summary>
        /// <param name="path"></param>
        /// <param name="waitingTime"></param>
        internal void npmRestore(string path, TimeSpan waitingTime)
        {
            _processManager.runProcess(path, "npm", "i", waitingTime);
        }
    }
}
