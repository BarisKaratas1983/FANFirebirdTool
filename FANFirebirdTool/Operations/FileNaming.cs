using System.IO;

namespace FANFirebirdTool.Operations
{
    public static class FileNaming
    {
        public static string SuggestBackupPath(string databasePath)
        {
            string ext = Path.GetExtension(databasePath).ToLowerInvariant();
            return Path.ChangeExtension(databasePath, ext == ".gdb" ? ".gbk" : ".fbk");
        }
        public static string SuggestRestorePath(string backupPath)
        {
            string ext = Path.GetExtension(backupPath).ToLowerInvariant();
            return Path.ChangeExtension(backupPath, ext == ".gbk" ? ".gdb" : ".fdb");
        }
    }
}