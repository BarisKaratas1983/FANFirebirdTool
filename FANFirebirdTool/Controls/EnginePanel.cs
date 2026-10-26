using FANFirebirdTool.Engines;
using FANFirebirdTool.Operations;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace FANFirebirdTool.Controls
{
    [DesignerCategory("Code")]
    public sealed class EnginePanel : TableLayoutPanel
    {
        private readonly ComboBox cmbEngine = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox txtUser = new TextBox { Text = "SYSDBA" };
        private readonly TextBox txtPassword = new TextBox { Text = "masterkey", UseSystemPasswordChar = true };
        public event EventHandler EngineChanged;
        public EnginePanel()
        {
            cmbEngine.SelectedIndexChanged += (s, e) =>
            {
                var handler = EngineChanged;
                if (handler != null) handler(this, EventArgs.Empty);
            };

            Ui.SetupFormTable(this, 2);
            Ui.AddRow(this, "ui.engine", cmbEngine);
            Ui.AddRow(this, "ui.user", txtUser);
            Ui.AddRow(this, "ui.password", txtPassword);

            cmbEngine.Items.AddRange(EngineCatalog.Available.Cast<object>().ToArray());
            if (cmbEngine.Items.Count > 0)
                cmbEngine.SelectedIndex = cmbEngine.Items.Count - 1;
        }
        public FirebirdEngine SelectedEngine
        {
            get { return cmbEngine.SelectedItem as FirebirdEngine; }
        }
        public void ApplyTo(OperationOptions o)
        {
            o.Engine = SelectedEngine;
            o.User = txtUser.Text.Trim();
            o.Password = txtPassword.Text;
        }
    }
}