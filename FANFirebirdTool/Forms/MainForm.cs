using FANFirebirdTool.Controls;
using FANFirebirdTool.Core;
using FANFirebirdTool.Engines;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FANFirebirdTool.Forms
{
    [DesignerCategory("Code")]
    public sealed class MainForm : ThemedForm
    {
        private readonly Label lblTitle = new Label { Text = AppInfo.Name, AutoSize = true, Anchor = AnchorStyles.Left };
        private readonly Label lblSubtitle = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(5, 2, 3, 3) };
        private readonly Button btnTheme;
        private readonly Button btnAbout;
        private readonly MenuTile tileBackup = new MenuTile { IconName = "backup" };
        private readonly MenuTile tileRestore = new MenuTile { IconName = "restore" };
        private readonly MenuTile tileCreate = new MenuTile { IconName = "create" };
        private readonly MenuTile tileScript = new MenuTile { IconName = "script" };
        private readonly Label lblLanguage = new Label { AutoSize = true, Anchor = AnchorStyles.Left };
        private readonly ComboBox cmbLanguage = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130, Anchor = AnchorStyles.Left };
        private readonly Label lblEngines = new Label { Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(16, 3, 3, 3) };
        private readonly Label lblVersion = new Label { AutoSize = true, Anchor = AnchorStyles.Right };
        public MainForm()
        {
            Text = AppInfo.Name;
            Font = SystemFonts.MessageBoxFont;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Size = LogicalToDeviceUnits(new Size(700, 500));
            Padding = new Padding(20, 16, 20, 12);

            lblTitle.Font = new Font(Font.FontFamily, 18f, FontStyle.Bold);

            btnTheme = Ui.MakeButton("menu.theme", ToggleTheme);
            btnAbout = Ui.MakeButton("menu.about", ShowAbout);
            var headerButtons = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Anchor = AnchorStyles.Top | AnchorStyles.Right, Margin = Padding.Empty };
            headerButtons.Controls.Add(btnTheme);
            headerButtons.Controls.Add(btnAbout);

            var header = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(0, 0, 0, 14) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.Controls.Add(lblTitle, 0, 0);
            header.Controls.Add(lblSubtitle, 0, 1);
            header.Controls.Add(headerButtons, 1, 0);
            header.SetRowSpan(headerButtons, 2);

            var tiles = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tiles.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            tiles.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            tiles.Controls.Add(tileBackup, 0, 0);
            tiles.Controls.Add(tileRestore, 1, 0);
            tiles.Controls.Add(tileCreate, 0, 1);
            tiles.Controls.Add(tileScript, 1, 1);

            tileBackup.Click += (s, e) => Open(() => new BackupForm());
            tileRestore.Click += (s, e) => Open(() => new RestoreForm());
            tileCreate.Click += (s, e) => Open(() => new CreateForm());
            tileScript.Click += (s, e) => Open(() => new ScriptForm());

            var footer = new TableLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, ColumnCount = 4, Padding = new Padding(0, 10, 0, 0) };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.Controls.Add(lblLanguage, 0, 0);
            footer.Controls.Add(cmbLanguage, 1, 0);
            footer.Controls.Add(lblEngines, 2, 0);
            footer.Controls.Add(lblVersion, 3, 0);

            cmbLanguage.Items.AddRange(Loc.Languages.Cast<object>().ToArray());
            cmbLanguage.SelectedItem = Loc.Languages.FirstOrDefault(l => l.Code == Loc.CurrentCode);
            cmbLanguage.SelectedIndexChanged += (s, e) => ChangeLanguage();

            lblVersion.Text = "v" + AppInfo.Version + " (" + AppInfo.Architecture + ")";

            Controls.Add(tiles);
            Controls.Add(footer);
            Controls.Add(header);

            ApplyTexts();
        }
        private void ApplyTexts()
        {
            Palette p = Theme.Current;

            lblSubtitle.Text = Loc.T("app.subtitle");
            lblSubtitle.ForeColor = p.Muted;
            lblVersion.ForeColor = p.Muted;

            btnTheme.Text = Loc.T(Theme.IsDark ? "menu.lightTheme" : "menu.darkTheme");
            btnAbout.Text = Loc.T("menu.about");

            tileBackup.Title = Loc.T("backup.title");
            tileBackup.Description = Loc.T("menu.backup.desc");
            tileRestore.Title = Loc.T("restore.title");
            tileRestore.Description = Loc.T("menu.restore.desc");
            tileCreate.Title = Loc.T("create.title");
            tileCreate.Description = Loc.T("menu.create.desc");
            tileScript.Title = Loc.T("script.title");
            tileScript.Description = Loc.T("menu.script.desc");

            lblLanguage.Text = Loc.T("menu.language");

            var engines = EngineCatalog.Available.ToList();
            bool any = engines.Count > 0;
            lblEngines.Text = any
                ? Loc.T("menu.engines", string.Join(", ", engines.Select(x => x.DisplayName)))
                : Loc.T("menu.noEngines");
            lblEngines.ForeColor = any ? p.Muted : p.Error;

            foreach (MenuTile tile in new[] { tileBackup, tileRestore, tileCreate, tileScript })
                tile.Enabled = any;
        }
        private void ChangeLanguage()
        {
            var lang = cmbLanguage.SelectedItem as LanguageInfo;
            if (lang == null || lang.Code == Loc.CurrentCode) return;

            Loc.SetLanguage(lang.Code);
            Settings.Set("language", lang.Code);
            ApplyTexts();
        }
        private void ToggleTheme()
        {
            Theme.Toggle();
            Theme.Apply(this);
            ApplyTexts();
        }
        private void ShowAbout()
        {
            using (var f = new AboutForm())
                f.ShowDialog(this);
        }
        private void Open(Func<Form> create)
        {
            using (Form f = create())
                f.ShowDialog(this);
        }
    }
}