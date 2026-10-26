using System.IO;

namespace FANFirebirdTool.Core
{
    public static class Util
    {
        public static string Quote(string value)
        {
            return "\"" + value + "\"";
        }
        public static void SafeDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { }
        }
    }
}