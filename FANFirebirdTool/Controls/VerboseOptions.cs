using FANFirebirdTool.Core;
using System.ComponentModel;
using System.Windows.Forms;

namespace FANFirebirdTool.Controls
{
    [DesignerCategory("Code")]
    public sealed class VerboseOptions : FlowLayoutPanel
    {
        private readonly CheckBox chkVerbose = new CheckBox { Checked = true, AutoSize = true };
        private readonly CheckBox chkTime = new CheckBox { AutoSize = true, Margin = new Padding(20, 3, 3, 3) };
        public bool Verbose { get { return chkVerbose.Checked; } }
        public bool ShowTime { get { return chkVerbose.Checked && chkTime.Checked; } }
        public VerboseOptions()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            WrapContents = false;
            Margin = Padding.Empty;

            chkVerbose.Text = Loc.T("ui.verbose");
            chkTime.Text = Loc.T("ui.showTime");
            chkVerbose.CheckedChanged += (s, e) => chkTime.Enabled = chkVerbose.Checked;

            Controls.Add(chkVerbose);
            Controls.Add(chkTime);
        }
    }
}