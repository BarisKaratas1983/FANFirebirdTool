using FANFirebirdTool.Core;
using System;
using System.Windows.Forms;

namespace FANFirebirdTool
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Loc.Initialize();
            Theme.Initialize();
            Application.Run(new Forms.MainForm());
        }
    }
}
