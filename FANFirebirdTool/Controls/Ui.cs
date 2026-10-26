using FANFirebirdTool.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FANFirebirdTool.Controls
{
    internal static class Ui
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string x, string y);
        private static readonly Dictionary<string, string> ButtonIcons = new Dictionary<string, string>
        {
            { "ui.start", "start" },
            { "ui.cancel", "cancel" },
            { "ui.browse", "browse" },
            { "ui.clear", "clear" },
            { "ui.copy", "copy" },
            { "menu.about", "about" },
            { "menu.theme", "theme" },
            { "script.addFolder", "addfolder" },
            { "script.addFiles", "addfile" },
            { "script.moveUp", "up" },
            { "script.moveDown", "down" },
            { "script.remove", "remove" }
        };
        public static int CompareLikeExplorer(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
        public static void SetupFormTable(TableLayoutPanel t, int columns)
        {
            t.AutoSize = true;
            t.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            t.Dock = DockStyle.Top;
            t.ColumnCount = columns;
            t.ColumnStyles.Clear();
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            if (columns > 2) t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        }
        public static TableLayoutPanel FormTable(int columns)
        {
            var t = new TableLayoutPanel();
            SetupFormTable(t, columns);
            return t;
        }
        public static void AddRow(TableLayoutPanel t, string labelKey, Control input, Control extra = null)
        {
            int row = t.RowStyles.Count;
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowCount = row + 1;

            if (labelKey != null)
                t.Controls.Add(new Label
                {
                    Text = Loc.T(labelKey),
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(3, 3, 10, 3)
                }, 0, row);

            input.Dock = DockStyle.Fill;
            t.Controls.Add(input, 1, row);

            if (extra != null) t.Controls.Add(extra, 2, row);
            else if (t.ColumnCount > 2) t.SetColumnSpan(input, 2);
        }
        public static Button MakeButton(string key)
        {
            var b = new Button
            {
                Text = Loc.T(key),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(90, 0),
                Padding = new Padding(6, 2, 6, 2)
            };

            string icon;
            if (ButtonIcons.TryGetValue(key, out icon))
            {
                b.Tag = icon;
                b.Image = Icons.Button(icon);
                b.ImageAlign = ContentAlignment.MiddleLeft;
                b.TextImageRelation = TextImageRelation.ImageBeforeText;
            }
            return b;
        }
        public static GroupBox Group(string key, Control content)
        {
            var g = new GroupBox
            {
                Text = Loc.T(key),
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(8, 6, 8, 8)
            };
            content.Dock = DockStyle.Top;
            g.Controls.Add(content);
            return g;
        }
        public static Control Spacer(int height)
        {
            return new Panel { Dock = DockStyle.Top, Height = height };
        }
        public static void EnableFileDrop(TextBox box, params string[] extensions)
        {
            box.AllowDrop = true;
            box.DragEnter += (s, e) =>
                e.Effect = GetDroppedFile(e.Data, extensions) != null ? DragDropEffects.Copy : DragDropEffects.None;
            box.DragDrop += (s, e) =>
            {
                string file = GetDroppedFile(e.Data, extensions);
                if (file != null) box.Text = file;
            };
        }
        private static string GetDroppedFile(IDataObject data, string[] extensions)
        {
            var files = data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length != 1) return null;

            string ext = Path.GetExtension(files[0]);
            foreach (string allowed in extensions)
                if (string.Equals(ext, allowed, StringComparison.OrdinalIgnoreCase))
                    return files[0];
            return null;
        }
        public static Button MakeButton(string key, Action onClick)
        {
            var b = MakeButton(key);
            b.Click += (s, e) => onClick();
            return b;
        }
        public static void BrowseOpen(IWin32Window owner, TextBox box, string filterKey)
        {
            using (var d = new OpenFileDialog())
            {
                d.Filter = Loc.T(filterKey);
                d.InitialDirectory = FolderOf(box.Text);
                if (d.ShowDialog(owner) == DialogResult.OK) box.Text = d.FileName;
            }
        }
        public static void BrowseSave(IWin32Window owner, TextBox box, string filterKey)
        {
            using (var d = new SaveFileDialog())
            {
                d.Filter = Loc.T(filterKey);
                d.OverwritePrompt = false;
                d.InitialDirectory = FolderOf(box.Text);
                d.FileName = FileNameOf(box.Text);
                if (d.ShowDialog(owner) == DialogResult.OK) box.Text = d.FileName;
            }
        }
        public static void LinkSuggestion(TextBox source, TextBox target, Func<string, string> suggest)
        {
            string last = "";
            source.TextChanged += (s, e) =>
            {
                string current = target.Text.Trim();
                if (current.Length > 0 && current != last) return;

                string src = source.Text.Trim();
                try { last = src.Length == 0 ? "" : suggest(src); }
                catch (ArgumentException) { last = ""; }
                target.Text = last;
            };
        }
        public static string FolderOf(string path)
        {
            try
            {
                string d = Path.GetDirectoryName((path ?? "").Trim());
                return Directory.Exists(d) ? d : "";
            }
            catch (ArgumentException) { return ""; }
        }
        public static string FileNameOf(string path)
        {
            try { return Path.GetFileName((path ?? "").Trim()); }
            catch (ArgumentException) { return ""; }
        }
    }
}