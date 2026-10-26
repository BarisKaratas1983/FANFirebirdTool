using FANFirebirdTool.Core;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;

namespace FANFirebirdTool.Forms
{
    [DesignerCategory("Code")]
    public class ThemedForm : Form
    {
        public ThemedForm()
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        protected override void OnLoad(EventArgs e)
        {
            Theme.Apply(this);
            base.OnLoad(e);
        }
    }
}