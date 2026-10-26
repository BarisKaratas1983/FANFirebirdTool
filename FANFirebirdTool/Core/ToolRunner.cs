using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FANFirebirdTool.Core
{
    public static class ToolRunner
    {
        public static async Task<int> RunAsync(
            string exePath,
            string arguments,
            string workingDirectory,
            Action<string> onLine,
            CancellationToken cancellationToken,
            string stdinText = null,
            IDictionary<string, string> environment = null)
        {
            var sync = SynchronizationContext.Current;
            Action<string> post = line =>
            {
                if (onLine == null) return;
                if (sync != null) sync.Post(_ => onLine(line), null);
                else onLine(line);
            };

            Encoding oem = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = oem,
                StandardErrorEncoding = oem
            };

            if (environment != null)
                foreach (var item in environment)
                    psi.EnvironmentVariables[item.Key] = item.Value;

            using (var process = new Process { StartInfo = psi })
            {
                process.OutputDataReceived += (s, e) => { if (e.Data != null) post(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) post(e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (!string.IsNullOrEmpty(stdinText)) process.StandardInput.Write(stdinText);
                process.StandardInput.Close();

                using (cancellationToken.Register(() => TryKill(process)))
                {
                    await Task.Run(() => process.WaitForExit());
                }

                cancellationToken.ThrowIfCancellationRequested();
                return process.ExitCode;
            }
        }
        private static void TryKill(Process p)
        {
            try { if (!p.HasExited) p.Kill(); } catch { }
        }
    }
}