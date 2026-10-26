using FANFirebirdTool.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FANFirebirdTool.Operations
{
    public sealed class BackupOptions : OperationOptions
    {
        public string DatabasePath { get; set; }
        public string BackupPath { get; set; }
        public bool Verbose { get; set; } = true;
        public bool Overwrite { get; set; }
    }
    public static class BackupOperation
    {
        public static async Task<OperationResult> RunAsync(
            BackupOptions o, Action<string> onLine, CancellationToken cancellationToken)
        {
            if (o.Engine == null || !o.Engine.IsAvailable)
                return OperationResult.Fail(Loc.T("err.engineMissing"));
            if (string.IsNullOrWhiteSpace(o.DatabasePath) || !File.Exists(o.DatabasePath))
                return OperationResult.Fail(Loc.T("err.databaseNotFound"));
            if (string.IsNullOrWhiteSpace(o.BackupPath))
                return OperationResult.Fail(Loc.T("err.backupNameMissing"));

            string db = Path.GetFullPath(o.DatabasePath);
            string bak = Path.GetFullPath(o.BackupPath);

            if (string.Equals(db, bak, StringComparison.OrdinalIgnoreCase))
                return OperationResult.Fail(Loc.T("err.backupSameAsDatabase"));
            if (!Directory.Exists(Path.GetDirectoryName(bak)))
                return OperationResult.Fail(Loc.T("err.backupFolderMissing"));
            if (File.Exists(bak) && !o.Overwrite)
                return OperationResult.Fail(Loc.T("err.backupExists", bak));

            string temp = bak + ".fan-tmp";
            Util.SafeDelete(temp);

            string args = "-b " + (o.Verbose ? "-v " : "") + Util.Quote(db) + " " + Util.Quote(temp);

            try
            {
                int code = await ToolRunner.RunAsync(o.Engine.GbakPath, args, o.Engine.Folder,
                    onLine, cancellationToken, null, o.BuildEnvironment());

                if (code != 0)
                {
                    Util.SafeDelete(temp);
                    return OperationResult.Fail(Loc.T("err.gbakFailed", code));
                }

                if (File.Exists(bak)) File.Delete(bak);
                File.Move(temp, bak);
                return OperationResult.Ok(Loc.T("ok.backupDone", bak));
            }
            catch (OperationCanceledException)
            {
                Util.SafeDelete(temp);
                throw;
            }
        }
    }
}