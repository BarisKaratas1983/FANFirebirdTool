using FANFirebirdTool.Controls;
using FANFirebirdTool.Core;
using FANFirebirdTool.Engines;
using FANFirebirdTool.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FANFirebirdTool.Forms
{
    [DesignerCategory("Code")]
    public sealed class ScriptForm : OperationForm
    {
        private const int StatusColumn = 2;
        private readonly TextBox txtDatabase = new TextBox();
        private readonly ComboBox cmbCharset = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox chkContinue = new CheckBox { AutoSize = true };
        private readonly CheckBox chkTime = new CheckBox { AutoSize = true, Margin = new Padding(20, 3, 3, 3) };
        private readonly ListView lvScripts = new ListView();
        private readonly List<Button> listButtons = new List<Button>();
        private static readonly int[] MinColumnWidths = { 40, 150, 110 };
        private ScriptRunOptions options;
        public ScriptForm() : base("script.title")
        {
            Size = new Size(820, 780);
            MinimumSize = new Size(640, 660);

            cmbCharset.Items.AddRange(CharacterSets.All);
            cmbCharset.SelectedItem = "UTF8";
            chkContinue.Text = Loc.T("script.continueOnError");
            chkTime.Text = Loc.T("ui.showTime");

            var flags = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
            flags.Controls.Add(chkContinue);
            flags.Controls.Add(chkTime);

            var target = Ui.FormTable(3);
            Ui.AddRow(target, "script.database", txtDatabase,
                Ui.MakeButton("ui.browse", () => Ui.BrowseOpen(this, txtDatabase, "filter.database")));
            Ui.AddRow(target, "script.charset", cmbCharset);
            Ui.AddRow(target, null, flags);
            Ui.EnableFileDrop(txtDatabase, ".fdb", ".gdb");

            GroupBox grpScripts = Ui.Group("script.scripts", BuildScriptList());
            AddSections(Ui.Group("script.target", target), grpScripts);
            ExcludeFromLock(grpScripts);
        }
        private bool CanEdit
        {
            get { return !IsRunning; }
        }
        private Control BuildScriptList()
        {
            lvScripts.View = View.Details;
            lvScripts.FullRowSelect = true;
            lvScripts.HideSelection = false;
            lvScripts.MultiSelect = true;
            lvScripts.ShowItemToolTips = true;
            lvScripts.AllowDrop = true;
            lvScripts.Height = 220;
            lvScripts.Dock = DockStyle.Fill;

            lvScripts.OwnerDraw = true;
            lvScripts.DrawColumnHeader += DrawColumnHeader;
            lvScripts.DrawItem += (s, e) => e.DrawDefault = true;
            lvScripts.DrawSubItem += (s, e) => e.DrawDefault = true;

            lvScripts.Columns.Add(Loc.T("script.colNo"), 45, HorizontalAlignment.Right);
            lvScripts.Columns.Add(Loc.T("script.colFile"), 400);
            lvScripts.Columns.Add(Loc.T("script.colStatus"), 150);

            lvScripts.Resize += (s, e) => FitFileColumn();
            lvScripts.ColumnWidthChanging += (s, e) =>
            {
                if (e.NewWidth < MinColumnWidths[e.ColumnIndex])
                {
                    e.Cancel = true;
                    e.NewWidth = lvScripts.Columns[e.ColumnIndex].Width;
                }
            };

            lvScripts.ColumnWidthChanged += (s, e) =>
            {
                ColumnHeader col = lvScripts.Columns[e.ColumnIndex];
                if (col.Width < MinColumnWidths[e.ColumnIndex]) col.Width = MinColumnWidths[e.ColumnIndex];
            };

            lvScripts.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) RemoveSelected(); };
            lvScripts.DragEnter += (s, e) =>
                e.Effect = CanEdit && e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            lvScripts.DragDrop += (s, e) => AddPaths(e.Data.GetData(DataFormats.FileDrop) as string[]);

            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(6, 0, 0, 0)
            };
            AddListButton(buttons, "script.addFolder", AddFolder);
            AddListButton(buttons, "script.addFiles", AddFiles);
            AddListButton(buttons, "script.moveUp", () => MoveSelected(-1));
            AddListButton(buttons, "script.moveDown", () => MoveSelected(+1));
            AddListButton(buttons, "script.remove", RemoveSelected);
            AddListButton(buttons, "ui.clear", ClearList);

            var table = new TableLayoutPanel { AutoSize = true, ColumnCount = 2 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.Controls.Add(lvScripts, 0, 0);
            table.Controls.Add(buttons, 1, 0);
            return table;
        }
        private void DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (!Theme.IsDark)
            {
                e.DrawDefault = true;
                return;
            }

            Palette p = Theme.Current;
            using (var back = new SolidBrush(p.Surface))
                e.Graphics.FillRectangle(back, e.Bounds);
            using (var pen = new Pen(p.Border))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }

            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                (e.Header.TextAlign == HorizontalAlignment.Right ? TextFormatFlags.Right : TextFormatFlags.Left);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, lvScripts.Font, Rectangle.Inflate(e.Bounds, -6, 0), p.Text, flags);
        }
        private void AddListButton(Control parent, string key, Action onClick)
        {
            var b = Ui.MakeButton(key, onClick);
            b.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            listButtons.Add(b);
            parent.Controls.Add(b);
        }
        private void FitFileColumn()
        {
            int w = lvScripts.ClientSize.Width - lvScripts.Columns[0].Width - lvScripts.Columns[StatusColumn].Width;
            if (w > MinColumnWidths[1]) lvScripts.Columns[1].Width = w;
        }
        private void AddFolder()
        {
            using (var d = new FolderBrowserDialog())
            {
                d.Description = Loc.T("script.pickFolder");
                if (d.ShowDialog(this) == DialogResult.OK) AddPaths(new[] { d.SelectedPath });
            }
        }
        private void AddFiles()
        {
            using (var d = new OpenFileDialog())
            {
                d.Filter = Loc.T("filter.sql");
                d.Multiselect = true;
                if (d.ShowDialog(this) == DialogResult.OK) AddPaths(d.FileNames);
            }
        }
        private void AddPaths(string[] paths)
        {
            if (paths == null || !CanEdit) return;

            var files = new List<string>();
            foreach (string p in paths)
            {
                if (Directory.Exists(p)) files.AddRange(Directory.GetFiles(p, "*.sql"));
                else files.Add(p);
            }
            files.Sort(Ui.CompareLikeExplorer);

            var existing = new HashSet<string>(
                lvScripts.Items.Cast<ListViewItem>().Select(i => (string)i.Tag),
                StringComparer.OrdinalIgnoreCase);

            lvScripts.BeginUpdate();
            foreach (string f in files)
            {
                if (!IsSqlFile(f)) continue;
                string full = Path.GetFullPath(f);
                if (!existing.Add(full)) continue;

                var item = new ListViewItem(new[] { "", full, "" })
                {
                    Tag = full,
                    ToolTipText = full,
                    UseItemStyleForSubItems = false
                };
                lvScripts.Items.Add(item);
            }
            lvScripts.EndUpdate();
            Renumber();
        }
        private static bool IsSqlFile(string path)
        {
            return string.Equals(Path.GetExtension(path), ".sql", StringComparison.OrdinalIgnoreCase);
        }
        private void MoveSelected(int delta)
        {
            if (!CanEdit) return;
            var indices = lvScripts.SelectedIndices.Cast<int>().OrderBy(i => i).ToList();
            if (indices.Count == 0) return;
            if (delta < 0 && indices[0] == 0) return;
            if (delta > 0 && indices[indices.Count - 1] == lvScripts.Items.Count - 1) return;

            var items = indices.Select(i => lvScripts.Items[i]).ToList();
            if (delta > 0) items.Reverse();

            lvScripts.BeginUpdate();
            foreach (ListViewItem item in items)
            {
                int index = item.Index;
                lvScripts.Items.RemoveAt(index);
                lvScripts.Items.Insert(index + delta, item);
            }
            foreach (ListViewItem item in items) item.Selected = true;
            lvScripts.EndUpdate();

            Renumber();
            items[0].EnsureVisible();
        }
        private void RemoveSelected()
        {
            if (!CanEdit) return;
            var indices = lvScripts.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            lvScripts.BeginUpdate();
            foreach (int i in indices) lvScripts.Items.RemoveAt(i);
            lvScripts.EndUpdate();
            Renumber();
        }
        private void ClearList()
        {
            if (!CanEdit) return;
            lvScripts.Items.Clear();
        }
        private void Renumber()
        {
            for (int i = 0; i < lvScripts.Items.Count; i++)
                lvScripts.Items[i].Text = (i + 1).ToString();
        }
        private void SetItemStatus(int index, string key, Color color)
        {
            ListViewItem.ListViewSubItem cell = lvScripts.Items[index].SubItems[StatusColumn];
            cell.Text = Loc.T(key);
            cell.Tag = key;
            cell.ForeColor = color;
        }
        private void OnUi(Action action)
        {
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }
        protected override bool PrepareToRun()
        {
            if (!CheckExistingFile(txtDatabase.Text, "err.databaseNameMissing", "err.databaseNotFound")) return false;
            if (lvScripts.Items.Count == 0) return Warn("err.noScripts");

            options = new ScriptRunOptions
            {
                DatabasePath = txtDatabase.Text.Trim(),
                ScriptFiles = lvScripts.Items.Cast<ListViewItem>().Select(i => (string)i.Tag).ToList(),
                CharacterSet = (string)cmbCharset.SelectedItem,
                ContinueOnError = chkContinue.Checked,
                FileStarted = i => OnUi(() =>
                {
                    SetItemStatus(i, "script.running", Theme.Current.Info);
                    lvScripts.Items[i].EnsureVisible();
                }),
                FileFinished = (i, ok) => OnUi(() =>
                    SetItemStatus(i, ok ? "script.ok" : "script.failed", ok ? Theme.Current.Success : Theme.Current.Error))
            };
            ApplyConnection(options);

            if (options.ScriptFiles.Count > 0 && File.Exists(options.DatabasePath))
            {
                string message = Loc.T("script.confirmRun", options.ScriptFiles.Count) +
                                 "\r\n\r\n" + Path.GetFullPath(options.DatabasePath);
                if (MessageBox.Show(this, message, Text, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return false;
            }

            for (int i = 0; i < lvScripts.Items.Count; i++)
                SetItemStatus(i, "script.waiting", Theme.Current.Muted);
            return true;
        }
        protected override Task<OperationResult> ExecuteAsync(Action<string> log, CancellationToken token)
        {
            return ScriptRunner.RunAsync(options, log, token);
        }
        protected override bool LogTimestamps
        {
            get { return chkTime.Checked; }
        }
        protected override void OnRunningChanged(bool running)
        {
            foreach (Button b in listButtons) b.Enabled = !running;
        }
        protected override void OnRunFinished()
        {
            int ok = 0, failed = 0, notRun = 0;
            string firstFailed = null;

            foreach (ListViewItem item in lvScripts.Items)
            {
                string key = item.SubItems[StatusColumn].Tag as string;
                if (key == "script.waiting")
                {
                    SetItemStatus(item.Index, "script.notRun", Theme.Current.Muted);
                    key = "script.notRun";
                }
                else if (key == "script.running")
                {
                    SetItemStatus(item.Index, "script.interrupted", Theme.Current.Warning);
                    key = "script.interrupted";
                }

                if (key == "script.ok") ok++;
                else if (key == "script.notRun") notRun++;
                else if (key == "script.failed")
                {
                    failed++;
                    if (firstFailed == null) firstFailed = (string)item.Tag;
                }
            }

            if (LastOutcome != RunOutcome.Completed || ok + failed == 0) return;

            string text;
            MessageBoxIcon icon;

            if (notRun > 0)
            {
                text = Loc.T("script.resultStopped", Path.GetFileName(firstFailed));
                icon = MessageBoxIcon.Error;
            }
            else if (failed == 0)
            {
                text = Loc.T("script.resultAllOk");
                icon = MessageBoxIcon.Information;
            }
            else if (ok == 0)
            {
                text = Loc.T("script.resultAllFailed");
                icon = MessageBoxIcon.Error;
            }
            else
            {
                text = Loc.T("script.resultMixed");
                icon = MessageBoxIcon.Warning;
            }

            string counts = notRun > 0
                ? Loc.T("script.resultCountsNotRun", ok, failed, notRun)
                : Loc.T("script.resultCounts", ok, failed);

            MessageBox.Show(this, text + "\r\n\r\n" + counts, Text, MessageBoxButtons.OK, icon);
        }
    }
}