using System.IO;

namespace FANFirebirdTool.Engines
{
    public sealed class FirebirdEngine
    {
        public string Id { get; private set; }
        public string DisplayName { get; private set; }
        public int OdsMajor { get; private set; }
        public string Folder { get; private set; }
        public FirebirdEngine(string id, string displayName, int odsMajor, string enginesRoot)
        {
            Id = id;
            DisplayName = displayName;
            OdsMajor = odsMajor;
            Folder = Path.Combine(enginesRoot, id);
        }
        public string GbakPath { get { return Path.Combine(Folder, "gbak.exe"); } }
        public string IsqlPath { get { return Path.Combine(Folder, "isql.exe"); } }
        public bool IsAvailable
        {
            get { return File.Exists(GbakPath) && File.Exists(IsqlPath); }
        }
        public override string ToString()
        {
            return DisplayName;
        }
        public int[] PageSizes
        {
            get
            {
                return OdsMajor >= 13
                    ? new[] { 4096, 8192, 16384, 32768 }
                    : new[] { 4096, 8192, 16384 };
            }
        }
    }
}