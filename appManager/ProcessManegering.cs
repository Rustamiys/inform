﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace appManager
{
    internal class ProcessManegering
    {
        internal static async Task errorHandler(string path, string command, TimeSpan processWaitingTime, int closeWindowTime)
        {
            IusManager.logger.Info($"Start process {command}");
            try
            {
                await runProcessWithTimeoutAsync(path, command, processWaitingTime, closeWindowTime);
                IusManager.logger.Info($"Finish process {command}");
            }
            catch (Exception ex)
            {
                IusManager.logger.Error($"Command:{command}, end with error: {ex.Message}");
            }
        }
        private static async Task runProcessWithTimeoutAsync(string path, string command, TimeSpan processWaitingTime, int closeWindowTime)
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

        private static void runProcess(string path, string command, int closeWindowTime, CancellationToken token)
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
