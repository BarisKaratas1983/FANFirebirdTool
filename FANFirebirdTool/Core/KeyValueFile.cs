using System;
using System.Collections.Generic;
using System.IO;

namespace FANFirebirdTool.Core
{
    internal static class KeyValueFile
    {
        public static Dictionary<string, string> Parse(TextReader reader, StringComparer comparer)
        {
            var table = new Dictionary<string, string>(comparer);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                table[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
            }
            return table;
        }
    }
}