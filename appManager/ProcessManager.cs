using System;
using System.Diagnostics;

using NLog;

namespace appManager
{
    /// <summary>
    /// Класс управления процессами (запуск/остановка процессов, отлов ошибок)
    /// </summary>
    internal class ProcessManager
    {
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Запускает процесс, отлавливает ошибки запуска
        /// </summary>
        /// <param name="workinDirectory"></param>
        /// <param name="fileName"></param>
        /// <param name="command"></param>
        /// <param name="waitingTime"></param>
        internal void tryRunProcess(string workinDirectory, string fileName, string command, TimeSpan waitingTime)
        {
            try
            {
                runProcess(workinDirectory, fileName, command, waitingTime);
            }
            catch (Exception exc)
            {
                _logger.Error($"Exception: {exc.Message}");
            }
        }
        /// <summary>
        /// Запускает процесс, время ожидания = waitingTime
        /// </summary>
        /// <param name="workingDirectory"> </param>
        /// <param name="fileName"></param>
        /// <param name="command"></param>
        /// <param name="waitingTime"></param>
        internal void runProcess(string workingDirectory, string fileName, string command, TimeSpan waitingTime)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = true,
                CreateNoWindow = false,
                WorkingDirectory = workingDirectory,
                Arguments = command,
                WindowStyle = ProcessWindowStyle.Normal
            };

            using (var process = new Process())
            {
                _logger.Info($"Start process {fileName} {command}");
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit(System.Convert.ToInt32(waitingTime.TotalMilliseconds));                
                _logger.Info($"Finish process {fileName} {command}");
            }
        }
    }
}
