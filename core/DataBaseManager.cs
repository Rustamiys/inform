using System;

namespace core
{
    /// <summary>
    /// Класс менеджера базы данных
    /// </summary>
    internal class DataBaseManager
    {
        private ProcessManager _ProcessManager = new ProcessManager();
        /// <summary>
        /// задача выполнения резервного копирования и восстановления backup-and-restore.cmd
        /// </summary>
        /// <param name="path"></param>
        /// <param name="waitingTime"></param>
        internal void backupAndRestore(string path, TimeSpan waitingTime)
        {
            _ProcessManager.tryRunProcess(path, "backup-and-restore.cmd", "", waitingTime);
        }
        /// <summary>
        /// задача выполнения обновления базы данных update-db.cmd $"{dbname} -w
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dbname"></param>
        /// <param name="waitingTime"></param>
        internal void updateDB(string path, string dbname, TimeSpan waitingTime)
        {
            _ProcessManager.RunProcess(path, "update-db.cmd", $"{dbname} -w", waitingTime);
        }
    }
}
