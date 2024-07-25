using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using NLog;

namespace appManager
{
    internal class ProcessManager
    {
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        internal void tryRunProcess(string workinDirectory, string fileName, string command)
        {
            try
            {
                runProcess(workinDirectory, fileName, command);
            }
            catch (Exception exc)
            {
                _logger.Error($"Exception: {exc.Message}");
            }
        }
        internal void runProcess(string workingDirectory, string fileName, string command)
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
                process.WaitForExit(100000);
                _logger.Info($"Finish process {fileName} {command}");
            }
        }
    }
}
