using FANFirebirdTool.Core;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FANFirebirdTool.Controls
{
    [DesignerCategory("Code")]
    public sealed class LogPanel : UserControl
    {
        private readonly TextBox txt = new TextBox();
        private readonly ConcurrentQueue<string> pending = new ConcurrentQueue<string>();
        private readonly System.Windows.Forms.Timer flushTimer = new System.Windows.Forms.Timer { Interval = 100 };
        public bool ShowTime { get; set; }
        public LogPanel()
        {
            Dock = DockStyle.Fill;

            txt.Multiline = true;
            txt.ReadOnly = true;
            txt.WordWrap = false;
            txt.ScrollBars = ScrollBars.Both;
            txt.MaxLength = 0;
            txt.Font = new Font("Consolas", 9f);
            txt.Dock = DockStyle.Fill;

            var caption = new Label { Text = Loc.T("ui.output"), AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0, 0, 0, 3) };

            var btnClear = Ui.MakeButton("ui.clear");
            var btnCopy = Ui.MakeButton("ui.copy");
            btnClear.Click += (s, e) => Clear();
            btnCopy.Click += (s, e) => { Flush(); if (txt.TextLength > 0) Clipboard.SetText(txt.Text); };

            var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            bar.Controls.Add(btnCopy);
            bar.Controls.Add(btnClear);

            Controls.Add(txt);
            Controls.Add(bar);
            Controls.Add(caption);

            flushTimer.Tick += (s, e) => Flush();
            flushTimer.Start();
        }
        public void AppendLine(string line)
        {
            line = line ?? "";
            if (ShowTime && line.Length > 0)
                line = DateTime.Now.ToString("HH:mm:ss") + "  " + line;
            pending.Enqueue(line);
        }
        public void Flush()
        {
            if (pending.IsEmpty) return;
            var sb = new StringBuilder();
            string line;
            while (pending.TryDequeue(out line)) sb.Append(line).Append("\r\n");
            txt.AppendText(sb.ToString());
        }
        public void Clear()
        {
            string dummy;
            while (pending.TryDequeue(out dummy)) { }
            txt.Clear();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) flushTimer.Dispose();
            base.Dispose(disposing);
        }
    }
}