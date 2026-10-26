using FANFirebirdTool.Controls;
using FANFirebirdTool.Core;
using FANFirebirdTool.Engines;
using FANFirebirdTool.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FANFirebirdTool.Forms
{
    [DesignerCategory("Code")]
    public abstract class OperationForm : ThemedForm
    {
        private readonly EnginePanel enginePanel = new EnginePanel();
        private readonly LogPanel logPanel = new LogPanel();
        private readonly Button btnStart = Ui.MakeButton("ui.start");
        private readonly Button btnCancel = Ui.MakeButton("ui.cancel");
        private readonly Label lblStatus = new Label { Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        private readonly List<Control> sections = new List<Control>();
        private CancellationTokenSource cts;
        private bool closeAfterCancel;
        protected enum RunOutcome { Completed, Cancelled, UnexpectedError }
        protected RunOutcome LastOutcome { get; private set; }
        protected OperationForm(string titleKey)
        {
            Text = Loc.T(titleKey);
            Font = SystemFonts.MessageBoxFont;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(760, 580);
            MinimumSize = new Size(560, 440);
            Padding = new Padding(10);

            var bottom = new TableLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, ColumnCount = 3, Padding = new Padding(0, 8, 0, 0) };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottom.Controls.Add(lblStatus, 0, 0);
            bottom.Controls.Add(btnStart, 1, 0);
            bottom.Controls.Add(btnCancel, 2, 0);

            btnStart.Click += (s, e) => Start();
            btnCancel.Click += (s, e) => ConfirmAndCancel();
            AcceptButton = btnStart;

            logPanel.Padding = new Padding(0, 8, 0, 0);
            Controls.Add(logPanel);
            Controls.Add(bottom);

            btnCancel.Enabled = false;
            SetStatus(Loc.T("ui.ready"), Theme.Current.Text);
            enginePanel.EngineChanged += (s, e) => OnEngineChanged();
        }
        protected bool Warn(string key, params object[] args)
        {
            MessageBox.Show(this, Loc.T(key, args), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        protected bool CheckExistingFile(string path, string missingKey, string notFoundKey)
        {
            path = (path ?? "").Trim();
            if (path.Length == 0) return Warn(missingKey);
            if (!File.Exists(path)) return Warn(notFoundKey);
            return true;
        }
        protected bool IsRunning
        {
            get { return cts != null; }
        }
        protected void ExcludeFromLock(Control section)
        {
            sections.Remove(section);
        }
        protected virtual void OnRunningChanged(bool running) { }
        protected virtual void OnRunFinished() { }
        protected FirebirdEngine SelectedEngine
        {
            get { return enginePanel.SelectedEngine; }
        }
        protected virtual void OnEngineChanged() { }
        protected virtual bool LogTimestamps
        {
            get { return false; }
        }
        protected void AddSections(params Control[] list)
        {
            var all = new List<Control> { Ui.Group("ui.connection", enginePanel) };
            all.AddRange(list);
            sections.AddRange(all);

            for (int i = all.Count - 1; i >= 0; i--)
            {
                Controls.Add(all[i]);
                if (i > 0) Controls.Add(Ui.Spacer(8));
            }
        }
        protected void ApplyConnection(OperationOptions o)
        {
            enginePanel.ApplyTo(o);
        }
        protected bool ConfirmOverwrite(string messageKey, string path)
        {
            return MessageBox.Show(this, Loc.T(messageKey) + "\r\n\r\n" + path, Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }
        protected abstract bool PrepareToRun();
        protected abstract Task<OperationResult> ExecuteAsync(Action<string> log, CancellationToken token);
        private async void Start()
        {
            if (cts != null || !PrepareToRun()) return;

            logPanel.Clear();
            logPanel.ShowTime = LogTimestamps;
            cts = new CancellationTokenSource();
            SetRunning(true);
            var sw = Stopwatch.StartNew();

            try
            {
                OperationResult r = await ExecuteAsync(logPanel.AppendLine, cts.Token);
                LastOutcome = RunOutcome.Completed;
                Finish(r.Success, r.Message, sw.Elapsed);
            }
            catch (OperationCanceledException)
            {
                LastOutcome = RunOutcome.Cancelled;
                Finish(false, Loc.T("ui.cancelled"), sw.Elapsed);
            }
            catch (Exception ex)
            {
                LastOutcome = RunOutcome.UnexpectedError;
                Finish(false, Loc.T("ui.unexpectedError", ex.Message), sw.Elapsed);
            }
            finally
            {
                cts.Dispose();
                cts = null;
                SetRunning(false);
                OnRunFinished();
                if (closeAfterCancel) Close();
            }
        }
        private void Finish(bool ok, string message, TimeSpan elapsed)
        {
            logPanel.AppendLine("");
            logPanel.AppendLine("==> " + message);
            logPanel.AppendLine(Loc.T("ui.elapsed", elapsed.TotalSeconds.ToString("0.0")));
            SetStatus(message, ok ? Theme.Current.Success : Theme.Current.Error);
        }
        private void ConfirmAndCancel()
        {
            if (cts == null) return;

            var answer = MessageBox.Show(this, Loc.T("ui.confirmCancel"), Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (answer == DialogResult.Yes) RequestCancel();
        }
        private void RequestCancel()
        {
            if (cts == null) return;
            btnCancel.Enabled = false;
            SetStatus(Loc.T("ui.cancelling"), Theme.Current.Text);
            cts.Cancel();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (cts != null)
            {
                e.Cancel = true;
                if (!closeAfterCancel &&
                    MessageBox.Show(this, Loc.T("ui.confirmCancelOnClose"), Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    closeAfterCancel = true;
                    RequestCancel();
                }
            }
            base.OnFormClosing(e);
        }
        private void SetRunning(bool running)
        {
            foreach (Control c in sections) c.Enabled = !running;
            btnStart.Enabled = !running;
            btnCancel.Enabled = running;
            if (running) SetStatus(Loc.T("ui.running"), Theme.Current.Text);
            OnRunningChanged(running);
        }
        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }
    }
}