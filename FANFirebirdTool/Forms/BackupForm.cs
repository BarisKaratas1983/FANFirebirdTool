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
    public sealed class BackupForm : OperationForm
    {
        private readonly TextBox txtDatabase = new TextBox();
        private readonly TextBox txtBackup = new TextBox();
        private readonly VerboseOptions verbose = new VerboseOptions();
        private BackupOptions options;
        protected override bool LogTimestamps
        {
            get { return verbose.ShowTime; }
        }
        public BackupForm() : base("backup.title")
        {
            var files = Ui.FormTable(3);
            Ui.AddRow(files, "backup.database", txtDatabase,
                Ui.MakeButton("ui.browse", () => Ui.BrowseOpen(this, txtDatabase, "filter.database")));
            Ui.AddRow(files, "backup.backupFile", txtBackup,
                Ui.MakeButton("ui.browse", () => Ui.BrowseSave(this, txtBackup, "filter.backup")));
            Ui.AddRow(files, null, verbose);

            Ui.EnableFileDrop(txtDatabase, ".fdb", ".gdb");
            Ui.LinkSuggestion(txtDatabase, txtBackup, FileNaming.SuggestBackupPath);

            AddSections(Ui.Group("ui.files", files));
        }
        protected override bool PrepareToRun()
        {
            if (!CheckExistingFile(txtDatabase.Text, "err.databaseNameMissing", "err.databaseNotFound")) return false;
            if (txtBackup.Text.Trim().Length == 0) return Warn("err.backupNameMissing");

            options = new BackupOptions
            {
                DatabasePath = txtDatabase.Text.Trim(),
                BackupPath = txtBackup.Text.Trim(),
                Verbose = verbose.Verbose
            };
            ApplyConnection(options);

            if (options.BackupPath.Length > 0 && File.Exists(options.BackupPath))
            {
                if (!ConfirmOverwrite("ui.confirmOverwrite", options.BackupPath)) return false;
                options.Overwrite = true;
            }
            return true;
        }
        protected override Task<OperationResult> ExecuteAsync(Action<string> log, CancellationToken token)
        {
            return BackupOperation.RunAsync(options, log, token);
        }
    }
}