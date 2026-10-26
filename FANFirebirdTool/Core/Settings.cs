using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FANFirebirdTool.Core
{
    public static class Settings
    {
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");
        private static readonly Dictionary<string, string> values = Load();
        public static string Get(string key)
        {
            string value;
            return values.TryGetValue(key, out value) ? value : null;
        }
        public static void Set(string key, string value)
        {
            values[key] = value;
            Save();
        }
        private static Dictionary<string, string> Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    using (var reader = new StreamReader(FilePath, Encoding.UTF8))
                        return KeyValueFile.Parse(reader, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { }

            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        private static void Save()
        {
            try
            {
                File.WriteAllLines(FilePath, values.Select(p => p.Key + "=" + p.Value), Encoding.UTF8);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { }
        }
    }
}