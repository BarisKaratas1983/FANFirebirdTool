using FANFirebirdTool.Controls;
using FANFirebirdTool.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FANFirebirdTool.Forms
{
    [DesignerCategory("Code")]
    public sealed class AboutForm : ThemedForm
    {
        private const int TextWidth = 400;
        public AboutForm()
        {
            Text = Loc.T("about.title");
            Font = SystemFonts.MessageBoxFont;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(20, 16, 20, 14);

            var layout = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Dock = DockStyle.Fill };
            var lblName = new Label { Text = AppInfo.Name, AutoSize = true, Font = new Font(Font.FontFamily, 14f, FontStyle.Bold) };
            var lblVersion = new Label { Text = Loc.T("about.version", AppInfo.Version, AppInfo.Architecture), AutoSize = true, ForeColor = Theme.Current.Muted, Margin = new Padding(3, 0, 3, 10) };
            var lblPurpose = WrappingLabel(Loc.T("about.purpose"));
            lblPurpose.Margin = new Padding(3, 0, 3, 12);

            var contact = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, Margin = new Padding(0, 0, 0, 12) };
            contact.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            contact.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            AddContactRow(contact, "about.developer", new Label { Text = AppInfo.Author, AutoSize = true });
            AddContactRow(contact, "about.email", Link(AppInfo.Email, "mailto:" + AppInfo.Email));
            AddContactRow(contact, "GitHub", Link(ShortUrl(AppInfo.GitHubUrl), AppInfo.GitHubUrl));
            AddContactRow(contact, "LinkedIn", Link(ShortUrl(AppInfo.LinkedInUrl), AppInfo.LinkedInUrl));
            AddContactRow(contact, "about.project", Link(ShortUrl(AppInfo.ProjectUrl), AppInfo.ProjectUrl));

            var lblLicense = WrappingLabel(Loc.T("about.license") + "  © " + AppInfo.Year + " " + AppInfo.Author);
            var lblFirebird = WrappingLabel(Loc.T("about.firebirdLicense"));
            lblFirebird.ForeColor = Theme.Current.Muted;

            var btnOk = Ui.MakeButton("ui.ok");
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Anchor = AnchorStyles.Right;
            btnOk.Margin = new Padding(3, 14, 3, 3);
            AcceptButton = btnOk;
            CancelButton = btnOk;

            layout.Controls.Add(lblName);
            layout.Controls.Add(lblVersion);
            layout.Controls.Add(lblPurpose);
            layout.Controls.Add(contact);
            layout.Controls.Add(lblLicense);
            layout.Controls.Add(lblFirebird);
            layout.Controls.Add(btnOk);
            Controls.Add(layout);
        }
        private static Label WrappingLabel(string text)
        {
            return new Label { Text = text, AutoSize = true, MaximumSize = new Size(TextWidth, 0) };
        }
        private static void AddContactRow(TableLayoutPanel t, string labelKey, Control value)
        {
            string text = labelKey.Contains(".") ? Loc.T(labelKey) : labelKey;
            t.Controls.Add(new Label { Text = text, AutoSize = true, ForeColor = Theme.Current.Muted, Margin = new Padding(3, 2, 14, 2) });
            value.Margin = new Padding(3, 2, 3, 2);
            t.Controls.Add(value);
        }
        private static LinkLabel Link(string text, string target)
        {
            var link = new LinkLabel { Text = text, AutoSize = true, LinkBehavior = LinkBehavior.HoverUnderline };
            link.LinkClicked += (s, e) =>
            {
                try { Process.Start(target); }
                catch (Exception ex) when (ex is Win32Exception || ex is InvalidOperationException) { }
            };
            return link;
        }
        private static string ShortUrl(string url)
        {
            return url.Replace("https://", "").Replace("www.", "");
        }
    }
}