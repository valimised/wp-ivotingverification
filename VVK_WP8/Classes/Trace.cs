using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VVK_WP8.Classes
{
    class Trace
    {
        public static void Log(string message,
            [CallerMemberName] string callerMember = "",
            [CallerFilePath] string callerPath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if(System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debug.WriteLine("-- {0} [called from {1} in {2} line {3}]", message, callerMember, callerPath, callerLineNumber);
        }
    }
}
