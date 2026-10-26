using System;
using System.Reflection;

namespace FANFirebirdTool.Core
{
    public static class AppInfo
    {
        public const string Name = "FAN Firebird Tool";
        public const string Author = "Barış Karataş";
        public const string Year = "2026";
        public const string Email = "baris.karatas@yahoo.com";
        public const string LinkedInUrl = "https://www.linkedin.com/in/baris-karatas";
        public const string GitHubUrl = "https://github.com/BarisKaratas1983";
        public const string ProjectUrl = "https://github.com/BarisKaratas1983/FANFirebirdTool";
        public static string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(3); }
        }
        public static string Architecture
        {
            get { return Environment.Is64BitProcess ? "x64" : "x86"; }
        }
    }
}