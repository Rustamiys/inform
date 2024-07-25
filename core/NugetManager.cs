using System;
using System.Collections.Generic;
using System.Text;

namespace core
{
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
            _ProcessManager.runProcess(path, "nuget", "restore", waitingTime);
        }
    }
}
