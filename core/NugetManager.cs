using System;
using System.Collections.Generic;
using System.Text;

namespace core
{
    /// <summary>
    /// Класс менеджера nuget пакетов
    /// </summary>
    internal class NugetManager
    {
        private ProcessManager _ProcessManager = new ProcessManager();
        /// <summary>
        /// задача восстановления nuget пакетов
        /// </summary>
        /// <param name="path"></param>
        /// <param name="waitingTime"></param>
        internal void restoreNuget(string path, TimeSpan waitingTime)
        {
            _ProcessManager.RunProcess(path, "nuget", "restore", waitingTime);
        }
    }
}
