using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FANFirebirdTool.Engines
{
    public static class EngineCatalog
    {
        public static string EnginesRoot
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "engines"); }
        }
        public static IReadOnlyList<FirebirdEngine> All { get; private set; }
        static EngineCatalog()
        {
            All = new List<FirebirdEngine>
            {
                new FirebirdEngine("fb25", "Firebird 2.5", 11, EnginesRoot),
                new FirebirdEngine("fb30", "Firebird 3.0", 12, EnginesRoot),
                new FirebirdEngine("fb40", "Firebird 4.0", 13, EnginesRoot),
                new FirebirdEngine("fb50", "Firebird 5.0", 13, EnginesRoot)
            };
        }
        public static IEnumerable<FirebirdEngine> Available
        {
            get { return All.Where(e => e.IsAvailable); }
        }
    }
}