using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FANFirebirdTool.Core
{
    public sealed class LanguageInfo
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        public LanguageInfo(string code, string name)
        {
            Code = code;
            Name = name;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    public static class Loc
    {
        private const string FallbackCode = "en";
        private const string Marker = ".Languages.";
        private static readonly Dictionary<string, Dictionary<string, string>> tables =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> current = new Dictionary<string, string>();
        private static Dictionary<string, string> fallback = new Dictionary<string, string>();
        public static string CurrentCode { get; private set; }
        public static IReadOnlyList<LanguageInfo> Languages { get; private set; }
        public static void Initialize()
        {
            LoadAll();

            string saved = Settings.Get("language");
            string windows = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (saved != null && tables.ContainsKey(saved)) SetLanguage(saved);
            else if (tables.ContainsKey(windows)) SetLanguage(windows);
            else SetLanguage(FallbackCode);
        }
        public static void SetLanguage(string code)
        {
            Dictionary<string, string> table;
            if (code == null || !tables.TryGetValue(code, out table))
            {
                code = FallbackCode;
                table = fallback;
            }
            current = table;
            CurrentCode = code;
        }
        public static string T(string key, params object[] args)
        {
            string text;
            if (!current.TryGetValue(key, out text) && !fallback.TryGetValue(key, out text))
                return "[" + key + "]";

            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        private static void LoadAll()
        {
            tables.Clear();
            Assembly asm = typeof(Loc).Assembly;

            foreach (string name in asm.GetManifestResourceNames())
            {
                int start = name.IndexOf(Marker, StringComparison.OrdinalIgnoreCase);
                if (start < 0 || !name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) continue;

                start += Marker.Length;
                string code = name.Substring(start, name.Length - start - ".txt".Length);

                using (Stream stream = asm.GetManifestResourceStream(name))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    tables[code] = KeyValueFile.Parse(reader, StringComparer.Ordinal);
            }

            Dictionary<string, string> en;
            fallback = tables.TryGetValue(FallbackCode, out en) ? en : new Dictionary<string, string>();

            Languages = tables
                .Select(p => new LanguageInfo(p.Key, p.Value.ContainsKey("language.name") ? p.Value["language.name"] : p.Key))
                .OrderBy(l => l.Name, StringComparer.CurrentCulture)
                .ToList();
        }
    }
}