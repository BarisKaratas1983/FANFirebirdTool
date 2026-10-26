using FANFirebirdTool.Core;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FANFirebirdTool.Controls
{
    [DesignerCategory("Code")]
    public sealed class MenuTile : Button
    {
        private string title = "";
        private string description = "";
        private string iconName;
        public MenuTile()
        {
            Text = "";
            Dock = DockStyle.Fill;
            Margin = new Padding(6);
            MinimumSize = new Size(200, 90);
            Cursor = Cursors.Hand;
        }
        public string Title
        {
            get { return title; }
            set { title = value ?? ""; AccessibleName = title; Invalidate(); }
        }
        public string Description
        {
            get { return description; }
            set { description = value ?? ""; AccessibleDescription = description; Invalidate(); }
        }
        public string IconName
        {
            get { return iconName; }
            set { iconName = value; Invalidate(); }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Palette p = Theme.Current;
            Rectangle area = Rectangle.Inflate(ClientRectangle, -16, -12);
            const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis;

            int iconSpace = 0;
            Image icon = iconName == null ? null : Icons.Tile(iconName);
            if (icon != null)
            {
                int x = area.Right - icon.Width, y = area.Bottom - icon.Height;
                if (Enabled) e.Graphics.DrawImage(icon, x, y, icon.Width, icon.Height);
                else ControlPaint.DrawImageDisabled(e.Graphics, icon, x, y, BackColor);
                iconSpace = icon.Width + 10;
            }

            using (var titleFont = new Font(Font.FontFamily, Font.Size * 1.4f, FontStyle.Bold))
            {
                int titleHeight = TextRenderer.MeasureText(e.Graphics, title, titleFont,
                    new Size(area.Width, int.MaxValue), TextFormatFlags.WordBreak).Height;

                var titleRect = new Rectangle(area.X, area.Y, area.Width, titleHeight);
                TextRenderer.DrawText(e.Graphics, title, titleFont, titleRect, Enabled ? ForeColor : p.Muted, flags);

                var descRect = new Rectangle(area.X, area.Y + titleHeight + 6,
                    area.Width - iconSpace, area.Height - titleHeight - 6);
                TextRenderer.DrawText(e.Graphics, description, Font, descRect, p.Muted, flags);
            }
        }
    }
}