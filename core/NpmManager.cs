using System;
using System.IO;

namespace core
{
    /// <summary>
    /// Класс npm менеджера
    /// </summary>
    internal class NpmManager
    {
        private FileManager _FileManager = new FileManager();
        private ProcessManager _ProcessManager = new ProcessManager();
        /// <summary>
        /// задача удаления node_modules
        /// </summary>
        /// <param name="path"></param>
        internal void removeNodeModules(string path)
        {
            var folder = new DirectoryInfo(path);
            _FileManager.cleanFiles(folder);
        }
        /// <summary>
        /// задача удаления package-lock.json 
        /// </summary>
        /// <param name="path"></param>
        internal void removePackageLock(string path)
        {
            _FileManager.cleanFiles(path);
        }
        /// <summary>
        /// задача восстановления node_modules
        /// </summary>
        /// <param name="path"></param>
        /// <param name="waitingTime"></param>
        internal void npmRestore(string path, TimeSpan waitingTime)
        {
            _ProcessManager.RunProcess(path, "npm", "i", waitingTime);
        }
    }
}
