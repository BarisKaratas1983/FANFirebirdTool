using FANFirebirdTool.Controls;
using FANFirebirdTool.Core;
using FANFirebirdTool.Operations;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FANFirebirdTool.Forms
{
    [DesignerCategory("Code")]
    public sealed class RestoreForm : OperationForm
    {
        private readonly TextBox txtBackup = new TextBox();
        private readonly TextBox txtDatabase = new TextBox();
        private readonly VerboseOptions verbose = new VerboseOptions();
        private RestoreOptions options;
        protected override bool LogTimestamps
        {
            get { return verbose.ShowTime; }
        }
        public RestoreForm() : base("restore.title")
        {
            var files = Ui.FormTable(3);
            Ui.AddRow(files, "restore.backupFile", txtBackup,
                Ui.MakeButton("ui.browse", () => Ui.BrowseOpen(this, txtBackup, "filter.backup")));
            Ui.AddRow(files, "restore.targetDatabase", txtDatabase,
                Ui.MakeButton("ui.browse", () => Ui.BrowseSave(this, txtDatabase, "filter.database")));
            Ui.AddRow(files, null, verbose);

            Ui.EnableFileDrop(txtBackup, ".fbk", ".gbk");
            Ui.LinkSuggestion(txtBackup, txtDatabase, FileNaming.SuggestRestorePath);

            AddSections(Ui.Group("ui.files", files));
        }
        protected override bool PrepareToRun()
        {
            if (!CheckExistingFile(txtBackup.Text, "err.backupNameMissing", "err.backupNotFound")) return false;
            if (txtDatabase.Text.Trim().Length == 0) return Warn("err.targetNameMissing");

            options = new RestoreOptions
            {
                BackupPath = txtBackup.Text.Trim(),
                DatabasePath = txtDatabase.Text.Trim(),
                Verbose = verbose.Verbose
            };
            ApplyConnection(options);

            if (options.DatabasePath.Length > 0 && File.Exists(options.DatabasePath))
            {
                if (!ConfirmOverwrite("restore.confirmOverwrite", options.DatabasePath)) return false;
                options.Overwrite = true;
            }
            return true;
        }
        protected override Task<OperationResult> ExecuteAsync(Action<string> log, CancellationToken token)
        {
            return RestoreOperation.RunAsync(options, log, token);
        }
    }
}