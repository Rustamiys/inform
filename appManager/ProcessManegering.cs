using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace appManager
{
    internal class ProcessManegering
    {
        internal static async Task errorHandler(string path, string command, TimeSpan timespan)
        {
            Console.WriteLine($"Start process {command}");
            try
            {
                await runProcessWithTimeoutAsync(path, command, timespan);
                Console.WriteLine($"Finish process {command}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        private static async Task runProcessWithTimeoutAsync(string path, string command, TimeSpan timespan)
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(timespan);
                try
                {
                    await Task.Run(() => runProcess(path, command, cts.Token), cts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException($"Время выполнения команды '{command}' истекло.");
                }
            }
        }

        private static void runProcess(string path, string command, CancellationToken token)
        {
            command = $"-NoExit -Command cd {path};{command}";
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = true,
                CreateNoWindow = false,
                WorkingDirectory = path,
                Arguments = command
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
