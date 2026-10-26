using FANFirebirdTool.Core;
using FANFirebirdTool.Engines;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FANFirebirdTool.Operations
{
    public sealed class CreateOptions : OperationOptions
    {
        public string DatabasePath { get; set; }
        public int PageSize { get; set; } = 8192;
        public string CharacterSet { get; set; } = "UTF8";
        public bool Overwrite { get; set; }
    }
    public static class CreateOperation
    {
        private static readonly Regex PromptPrefix = new Regex(@"^((SQL|CON)> )+", RegexOptions.Compiled);
        public static async Task<OperationResult> RunAsync(
            CreateOptions o, Action<string> onLine, CancellationToken cancellationToken)
        {
            if (o.Engine == null || !o.Engine.IsAvailable)
                return OperationResult.Fail(Loc.T("err.engineMissing"));
            if (string.IsNullOrWhiteSpace(o.DatabasePath))
                return OperationResult.Fail(Loc.T("err.databaseNameMissing"));
            if (Array.IndexOf(o.Engine.PageSizes, o.PageSize) < 0)
                return OperationResult.Fail(Loc.T("err.pageSizeInvalid"));
            if (string.IsNullOrEmpty(o.CharacterSet) || !CharacterSets.Contains(o.CharacterSet))
                return OperationResult.Fail(Loc.T("err.charsetInvalid"));

            string db = Path.GetFullPath(o.DatabasePath);
            if (!Directory.Exists(Path.GetDirectoryName(db)))
                return OperationResult.Fail(Loc.T("err.targetFolderMissing"));
            if (File.Exists(db) && !o.Overwrite)
                return OperationResult.Fail(Loc.T("err.databaseExists", db));

            string temp = db + ".fan-tmp";
            Util.SafeDelete(temp);

            string script =
                "CREATE DATABASE '" + temp.Replace("'", "''") + "' PAGE_SIZE " + o.PageSize +
                " DEFAULT CHARACTER SET " + o.CharacterSet + ";\r\n" +
                "QUIT;\r\n";

            bool failed = false;
            Action<string> filter = raw =>
            {
                string line = PromptPrefix.Replace(raw, "").TrimEnd();
                if (line.Length == 0 || line.StartsWith("Use CONNECT or CREATE DATABASE")) return;
                if (line.StartsWith("Statement failed")) failed = true;
                if (onLine != null) onLine(line);
            };

            try
            {
                int code = await ToolRunner.RunAsync(o.Engine.IsqlPath, "", o.Engine.Folder,
                    filter, cancellationToken, script, o.BuildEnvironment());

                if (failed || code != 0 || !File.Exists(temp))
                {
                    Util.SafeDelete(temp);
                    return OperationResult.Fail(Loc.T("err.createFailed"));
                }

                try
                {
                    if (File.Exists(db)) File.Delete(db);
                    File.Move(temp, db);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    return OperationResult.Fail(Loc.T("err.createReplaceFailed", temp));
                }

                return OperationResult.Ok(Loc.T("ok.createDone", db));
            }
            catch (OperationCanceledException)
            {
                Util.SafeDelete(temp);
                throw;
            }
        }
    }
}