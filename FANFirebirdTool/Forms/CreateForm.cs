using FANFirebirdTool.Controls;
using FANFirebirdTool.Core;
using FANFirebirdTool.Engines;
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
    public sealed class CreateForm : OperationForm
    {
        private const int DefaultPageSize = 8192;
        private readonly TextBox txtDatabase = new TextBox();
        private readonly ComboBox cmbPageSize = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cmbCharset = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private CreateOptions options;
        public CreateForm() : base("create.title")
        {
            cmbCharset.Items.AddRange(CharacterSets.All);
            cmbCharset.SelectedItem = "UTF8";
            FillPageSizes();

            var file = Ui.FormTable(3);
            Ui.AddRow(file, "create.database", txtDatabase,
                Ui.MakeButton("ui.browse", () => Ui.BrowseSave(this, txtDatabase, "filter.database")));

            var settings = Ui.FormTable(2);
            Ui.AddRow(settings, "create.pageSize", cmbPageSize);
            Ui.AddRow(settings, "create.charset", cmbCharset);

            AddSections(Ui.Group("ui.files", file), Ui.Group("create.settings", settings));
        }
        protected override void OnEngineChanged()
        {
            FillPageSizes();
        }
        private void FillPageSizes()
        {
            object previous = cmbPageSize.SelectedItem ?? DefaultPageSize;
            cmbPageSize.Items.Clear();

            FirebirdEngine engine = SelectedEngine;
            if (engine == null) return;

            foreach (int size in engine.PageSizes)
                cmbPageSize.Items.Add(size);

            cmbPageSize.SelectedItem = cmbPageSize.Items.Contains(previous)
                ? previous
                : cmbPageSize.Items[cmbPageSize.Items.Count - 1];
        }
        protected override bool PrepareToRun()
        {
            if (txtDatabase.Text.Trim().Length == 0) return Warn("err.databaseNameMissing");

            options = new CreateOptions
            {
                DatabasePath = txtDatabase.Text.Trim(),
                PageSize = cmbPageSize.SelectedItem == null ? 0 : (int)cmbPageSize.SelectedItem,
                CharacterSet = (string)cmbCharset.SelectedItem
            };
            ApplyConnection(options);

            if (options.DatabasePath.Length > 0 && File.Exists(options.DatabasePath))
            {
                if (!ConfirmOverwrite("create.confirmOverwrite", options.DatabasePath)) return false;
                options.Overwrite = true;
            }
            return true;
        }
        protected override Task<OperationResult> ExecuteAsync(Action<string> log, CancellationToken token)
        {
            return CreateOperation.RunAsync(options, log, token);
        }
    }
}