using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using NLog;

namespace appManager
{
    internal class ProcessManegering
    {
        private NLog.ILogger logger = LogManager.GetCurrentClassLogger();
        internal async Task errorHandler(string path, string command, TimeSpan processWaitingTime, int closeWindowTime)
        {
            logger.Info($"Start process {command}");
            try
            {
                await runProcessWithTimeoutAsync(path, command, processWaitingTime, closeWindowTime);
                logger.Info($"Finish process {command}");
            }
            catch (Exception ex)
            {
                logger.Error($"Command:{command}, end with error: {ex.Message}");
            }
        }
        private async Task runProcessWithTimeoutAsync(string path, string command, TimeSpan processWaitingTime, int closeWindowTime)
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(processWaitingTime);
                try
                {
                    await Task.Run(() => runProcess(path, command, closeWindowTime, cts.Token), cts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException($"Время выполнения команды '{command}' истекло.");
                }
            }
        }

        private void runProcess(string path, string command, int closeWindowTime, CancellationToken token)
        {
            command = $"-NoExit -Command cd {path};{command};Start-Sleep -Seconds {closeWindowTime};exit";
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = true,
                CreateNoWindow = false,
                WorkingDirectory = path,
                Arguments = command,
                WindowStyle = ProcessWindowStyle.Minimized
            };

            Process process = new Process();
            process.StartInfo = processInfo;
            process.Start();
            while (true)
            {
                if (process.HasExited)
                {
                    return;
                }
                else if (token.IsCancellationRequested)
                {
                    process.Kill();
                    throw new OperationCanceledException(token);
                }
            }
        }
    }
}
