using FANFirebirdTool.Engines;
using System.Collections.Generic;

namespace FANFirebirdTool.Operations
{
    public abstract class OperationOptions
    {
        public FirebirdEngine Engine { get; set; }
        public string User { get; set; } = "SYSDBA";
        public string Password { get; set; } = "masterkey";
        public IDictionary<string, string> BuildEnvironment()
        {
            var env = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(User)) env["ISC_USER"] = User;
            if (!string.IsNullOrEmpty(Password)) env["ISC_PASSWORD"] = Password;
            return env;
        }
    }
}