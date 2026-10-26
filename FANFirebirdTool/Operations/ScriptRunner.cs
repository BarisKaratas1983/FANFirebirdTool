using FANFirebirdTool.Core;
using FANFirebirdTool.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FANFirebirdTool.Operations
{
    public sealed class ScriptRunOptions : OperationOptions
    {
        public string DatabasePath { get; set; }
        public IList<string> ScriptFiles { get; set; }
        public string CharacterSet { get; set; } = "UTF8";
        public bool ContinueOnError { get; set; }
        public Action<int> FileStarted { get; set; }
        public Action<int, bool> FileFinished { get; set; }
    }
    public static class ScriptRunner
    {
        public static async Task<OperationResult> RunAsync(
            ScriptRunOptions o, Action<string> onLine, CancellationToken cancellationToken)
        {
            Action<string> log = onLine ?? (s => { });

            if (o.Engine == null || !o.Engine.IsAvailable)
                return OperationResult.Fail(Loc.T("err.engineMissing"));
            if (string.IsNullOrWhiteSpace(o.DatabasePath) || !File.Exists(o.DatabasePath))
                return OperationResult.Fail(Loc.T("err.databaseNotFound"));
            if (o.ScriptFiles == null || o.ScriptFiles.Count == 0)
                return OperationResult.Fail(Loc.T("err.noScripts"));
            if (string.IsNullOrEmpty(o.CharacterSet) || !CharacterSets.Contains(o.CharacterSet))
                return OperationResult.Fail(Loc.T("err.charsetInvalid"));
            
            foreach (string f in o.ScriptFiles)
                if (!File.Exists(f))
                    return OperationResult.Fail(Loc.T("err.scriptNotFound", f));

            string db = Path.GetFullPath(o.DatabasePath);
            string charset = o.CharacterSet.ToUpperInvariant();
            string tempDir = Path.Combine(Path.GetTempPath(), "FANScripts", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            int succeeded = 0, failed = 0, notRun = 0;
            try
            {
                int total = o.ScriptFiles.Count;
                for (int i = 0; i < total; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string original = Path.GetFullPath(o.ScriptFiles[i]);
                    string copy = Path.Combine(tempDir, "script_" + (i + 1).ToString("D3") + ".sql");

                    if (o.FileStarted != null) o.FileStarted(i);
                    log("=== [" + (i + 1) + "/" + total + "] " + Path.GetFileName(original) + " ===");

                    bool fileFailed = false;

                    try
                    {
                        PrepareCopy(original, copy, charset, log);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        log(Loc.T("log.scriptUnreadable", ex.Message));
                        fileFailed = true;
                    }

                    if (!fileFailed)
                    {
                        Action<string> filter = raw =>
                        {
                            if (raw.StartsWith("Statement failed")) fileFailed = true;
                            log(raw.Replace(copy, original));
                        };

                        string args = "-b" + (charset == "NONE" ? "" : " -ch " + charset) +
                                      " -i " + Util.Quote(copy) + " " + Util.Quote(db);

                        int code = await ToolRunner.RunAsync(o.Engine.IsqlPath, args, o.Engine.Folder,
                            filter, cancellationToken, null, o.BuildEnvironment());

                        if (code != 0) fileFailed = true;
                    }

                    if (o.FileFinished != null) o.FileFinished(i, !fileFailed);

                    if (fileFailed)
                    {
                        failed++;
                        log(Loc.T("log.scriptFailed", Path.GetFileName(original)));
                        if (!o.ContinueOnError)
                        {
                            notRun = total - i - 1;
                            break;
                        }
                    }
                    else
                    {
                        succeeded++;
                    }
                }
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }

            if (failed == 0)
                return OperationResult.Ok(Loc.T("ok.scriptsDone", succeeded));

            return OperationResult.Fail(notRun > 0
                ? Loc.T("res.scriptsSummaryNotRun", succeeded, failed, notRun)
                : Loc.T("res.scriptsSummary", succeeded, failed));
        }
        private static void PrepareCopy(string source, string copy, string charset, Action<string> log)
        {
            byte[] data = File.ReadAllBytes(source);

            int offset = 0;
            if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
                offset = 3;

            if (charset == "UTF8")
            {
                try
                {
                    int sample = Math.Min(data.Length - offset, 1024 * 1024);
                    new UTF8Encoding(false, true).GetDecoder().GetCharCount(data, offset, sample, false);
                }
                catch (DecoderFallbackException)
                {
                    log(Loc.T("log.notUtf8"));
                }
            }

            using (var fs = new FileStream(copy, FileMode.Create, FileAccess.Write))
                fs.Write(data, offset, data.Length - offset);
        }
    }
}