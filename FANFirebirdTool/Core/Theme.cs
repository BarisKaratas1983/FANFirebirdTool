using FANFirebirdTool.Controls;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FANFirebirdTool.Core
{
    public sealed class Palette
    {
        public Color Back { get; set; }
        public Color Surface { get; set; }
        public Color Input { get; set; }
        public Color Text { get; set; }
        public Color Muted { get; set; }
        public Color Border { get; set; }
        public Color Hover { get; set; }
        public Color Pressed { get; set; }
        public Color Link { get; set; }
        public Color Success { get; set; }
        public Color Error { get; set; }
        public Color Info { get; set; }
        public Color Warning { get; set; }
    }
    public static class Theme
    {
        private static readonly Palette Light = new Palette
        {
            Back = Color.Empty,
            Surface = Color.Empty,
            Input = SystemColors.Window,
            Text = Color.Empty,
            Muted = SystemColors.GrayText,
            Border = SystemColors.ControlDark,
            Hover = Color.Empty,
            Pressed = Color.Empty,
            Link = Color.Empty,
            Success = Color.ForestGreen,
            Error = Color.Firebrick,
            Info = Color.RoyalBlue,
            Warning = Color.DarkOrange
        };
        private static readonly Palette Dark = new Palette
        {
            Back = Color.FromArgb(32, 32, 32),
            Surface = Color.FromArgb(50, 50, 50),
            Input = Color.FromArgb(24, 24, 24),
            Text = Color.FromArgb(240, 240, 240),
            Muted = Color.FromArgb(165, 165, 165),
            Border = Color.FromArgb(80, 80, 80),
            Hover = Color.FromArgb(62, 62, 62),
            Pressed = Color.FromArgb(74, 74, 74),
            Link = Color.FromArgb(98, 171, 245),
            Success = Color.FromArgb(108, 203, 95),
            Error = Color.FromArgb(255, 138, 128),
            Info = Color.FromArgb(98, 171, 245),
            Warning = Color.FromArgb(247, 163, 92)
        };
        public static bool IsDark { get; private set; }
        public static Palette Current
        {
            get { return IsDark ? Dark : Light; }
        }

        public static void Initialize()
        {
            string saved = Settings.Get("theme");
            if (saved == "dark") SetDark(true);
            else if (saved == "light") SetDark(false);
            else SetDark(WindowsUsesDarkTheme());
        }
        public static void Toggle()
        {
            SetDark(!IsDark);
            Settings.Set("theme", IsDark ? "dark" : "light");
        }
        private static void SetDark(bool dark)
        {
            IsDark = dark;
            Icons.DarkTheme = dark;
        }
        private static bool WindowsUsesDarkTheme()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    object value = key == null ? null : key.GetValue("AppsUseLightTheme");
                    return value is int && (int)value == 0;
                }
            }
            catch (Exception ex) when (ex is System.Security.SecurityException || ex is UnauthorizedAccessException)
            {
                return false;
            }
        }
        public static void Apply(Control root)
        {
            Palette p = Current;
            var form = root as Form;
            
            if (form != null)
            {
                form.BackColor = p.Back;
                form.ForeColor = p.Text;
                ApplyTitleBar(form);
            }
            foreach (Control c in root.Controls) ApplyTo(c, p);
        }
        private static void ApplyTo(Control c, Palette p)
        {
            var button = c as Button;
            if (button != null) StyleButton(button, p);
            else if (c is TextBox || c is ListView)
            {
                c.BackColor = p.Input;
                c.ForeColor = p.Text;
                SetControlTheme(c, IsDark ? "DarkMode_Explorer" : null);
            }
            else if (c is ComboBox)
            {
                c.BackColor = p.Input;
                c.ForeColor = p.Text;
                SetControlTheme(c, IsDark ? "DarkMode_CFD" : null);
            }
            else if (c is GroupBox) c.ForeColor = p.Text;
            else if (c is LinkLabel)
            {
                var link = (LinkLabel)c;
                link.LinkColor = p.Link;
                link.ActiveLinkColor = p.Link;
                link.VisitedLinkColor = p.Link;
            }

            foreach (Control child in c.Controls) ApplyTo(child, p);
        }
        private static void StyleButton(Button b, Palette p)
        {
            if (IsDark)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.BackColor = p.Surface;
                b.ForeColor = p.Text;
                b.FlatAppearance.BorderColor = p.Border;
                b.FlatAppearance.MouseOverBackColor = p.Hover;
                b.FlatAppearance.MouseDownBackColor = p.Pressed;
            }
            else
            {
                b.FlatStyle = FlatStyle.Standard;
                b.BackColor = Color.Empty;
                b.ForeColor = Color.Empty;
                b.UseVisualStyleBackColor = true;
            }

            string icon = b.Tag as string;
            if (icon != null) b.Image = Icons.Button(icon);
            b.Invalidate();
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string subAppName, string idList);
        private static void ApplyTitleBar(Form form)
        {
            WhenHandleReady(form, handle =>
            {
                int value = IsDark ? 1 : 0;
                if (DwmSetWindowAttribute(handle, 20, ref value, sizeof(int)) != 0)
                    DwmSetWindowAttribute(handle, 19, ref value, sizeof(int));
            });
        }
        private static void SetControlTheme(Control c, string themeName)
        {
            WhenHandleReady(c, handle => SetWindowTheme(handle, themeName, null));
        }
        private static void WhenHandleReady(Control c, Action<IntPtr> action)
        {
            if (c.IsHandleCreated)
            {
                action(c.Handle);
                return;
            }
            EventHandler handler = null;
            handler = (s, e) =>
            {
                c.HandleCreated -= handler;
                action(c.Handle);
            };
            c.HandleCreated += handler;
        }
    }
}