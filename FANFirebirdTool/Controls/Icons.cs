using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;

namespace FANFirebirdTool.Controls
{
    internal static class Icons
    {
        private static readonly int[] ButtonSizes = { 16, 24, 32 };
        private static readonly int[] TileSizes = { 32, 48, 64 };
        private static readonly Dictionary<string, Image> cache = new Dictionary<string, Image>();
        private static readonly int systemDpi = ReadSystemDpi();
        public static bool DarkTheme { get; set; }
        public static Image Button(string name) { return Get(name, 16, ButtonSizes); }
        public static Image Tile(string name) { return Get(name, 48, TileSizes); }
        private static Image Get(string name, int logicalSize, int[] sizes)
        {
            int wanted = (int)Math.Round(logicalSize * systemDpi / 96.0);
            string variant = DarkTheme ? "dark" : "light";
            string cacheKey = name + "|" + wanted + "|" + variant;

            Image image;
            if (cache.TryGetValue(cacheKey, out image)) return image;

            int source = sizes[sizes.Length - 1];
            foreach (int s in sizes)
                if (s >= wanted) { source = s; break; }

            using (Image original = Load(name + "_" + source + "_" + variant + ".png"))
                image = original == null ? null : Resize(original, wanted);

            cache[cacheKey] = image;
            return image;
        }
        private static Image Load(string fileName)
        {
            Assembly asm = typeof(Icons).Assembly;
            foreach (string res in asm.GetManifestResourceNames())
            {
                if (!res.EndsWith(".Icons." + fileName, StringComparison.OrdinalIgnoreCase)) continue;
                using (Stream stream = asm.GetManifestResourceStream(res))
                using (Image img = Image.FromStream(stream))
                    return new Bitmap(img);
            }
            return null;
        }
        private static Image Resize(Image img, int size)
        {
            var bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(img, 0, 0, size, size);
            }
            return bmp;
        }
        private static int ReadSystemDpi()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                return (int)g.DpiX;
        }
    }
}