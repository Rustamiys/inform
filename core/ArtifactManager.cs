using System.IO;
using System;

namespace core
{
    /// <summary>
    /// Класс менеджера артефактов
    /// </summary>
    internal class ArtifactManager
    {
        private ProcessManager _ProcessManager = new ProcessManager();
        private FileManager _FileManager = new FileManager();
        /// <summary>
        /// задача выполнения kill.ps1
        /// </summary>
        /// <param name="path"></param>
        /// <param name="waitingTime"></param>
        internal void taskKill(string path, TimeSpan waitingTime)
        {
            _ProcessManager.RunProcess(path, "powershell.exe", "./kill.ps1", waitingTime);
        }
        /// <summary>
        /// задача выполнения удаления бинарных файлов remove-bin-obj.ps1
        /// </summary>
        /// <param name="path"></param>
        /// <param name="waitingTime"></param>
        internal void removeBinObj(string path, TimeSpan waitingTime)
        {
            _ProcessManager.RunProcess(path, "powershell.exe", "./remove-bin-obj.ps1", waitingTime);
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
                _FileManager.cleanFiles(folder);
            };
        }
    }
}
