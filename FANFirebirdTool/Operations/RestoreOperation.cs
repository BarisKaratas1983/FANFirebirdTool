using FANFirebirdTool.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FANFirebirdTool.Operations
{
    public sealed class RestoreOptions : OperationOptions
    {
        public string BackupPath { get; set; }
        public string DatabasePath { get; set; }
        public bool Verbose { get; set; } = true;
        public bool Overwrite { get; set; }
    }
    public static class RestoreOperation
    {
        public static async Task<OperationResult> RunAsync(
            RestoreOptions o, Action<string> onLine, CancellationToken cancellationToken)
        {
            if (o.Engine == null || !o.Engine.IsAvailable)
                return OperationResult.Fail(Loc.T("err.engineMissing"));
            if (string.IsNullOrWhiteSpace(o.BackupPath) || !File.Exists(o.BackupPath))
                return OperationResult.Fail(Loc.T("err.backupNotFound"));
            if (string.IsNullOrWhiteSpace(o.DatabasePath))
                return OperationResult.Fail(Loc.T("err.targetNameMissing"));

            string bak = Path.GetFullPath(o.BackupPath);
            string db = Path.GetFullPath(o.DatabasePath);

            if (!Directory.Exists(Path.GetDirectoryName(db)))
                return OperationResult.Fail(Loc.T("err.targetFolderMissing"));
            if (File.Exists(db) && !o.Overwrite)
                return OperationResult.Fail(Loc.T("err.targetExists", db));

            string temp = db + ".fan-tmp";
            Util.SafeDelete(temp);

            string args = "-c " + (o.Verbose ? "-v " : "") + Util.Quote(bak) + " " + Util.Quote(temp);

            try
            {
                int code = await ToolRunner.RunAsync(o.Engine.GbakPath, args, o.Engine.Folder,
                    onLine, cancellationToken, null, o.BuildEnvironment());

                if (code != 0)
                {
                    Util.SafeDelete(temp);
                    return OperationResult.Fail(Loc.T("err.gbakFailed", code));
                }

                try
                {
                    if (File.Exists(db)) File.Delete(db);
                    File.Move(temp, db);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    return OperationResult.Fail(Loc.T("err.restoreReplaceFailed", temp));
                }

                return OperationResult.Ok(Loc.T("ok.restoreDone", db));
            }
            catch (OperationCanceledException)
            {
                Util.SafeDelete(temp);
                throw;
            }
        }
    }
}