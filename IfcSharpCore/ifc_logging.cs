using NetSystem = System;
using System.IO;
using System.Linq;

namespace ifc
{

class IfcSharpException:NetSystem.Exception {public IfcSharpException (NetSystem.String reason) : base ("ifcSharp:"+reason){}}

    public static class Log
    {
        public enum Level
        {
            Info,
            Debug,
            Warning,
            Error,
            Exception,
            ThrowException
        };

        private static Level[] Filter = new[] {Level.Info};
        private const bool OutputToConsole = true;
        private static string logFilePath = "ifcsharp.log";
        public static void Add(string msg, Level level) {
            string log = $"{NetSystem.DateTime.Now:O}[{level}]: {msg}";
            if(OutputToConsole) NetSystem.Console.WriteLine(log);
            if (false == Filter.Contains(level)) {
                try {
                    using (StreamWriter sw = new StreamWriter(path: logFilePath, append: true)) {
                        sw.WriteLine(log);
                        sw.Close();
                    }

                }
                catch (NetSystem.Exception e) {
                    NetSystem.Console.WriteLine(e);
                }
            }
         if (level==Level.ThrowException) throw(new IfcSharpException(msg));  
        }

        public static void SetLogfilePath(string path) {
            logFilePath = path;
        }

        public static void SetFilter(Level[] filter) {
            Filter = filter;
        }
        public static void Reset() {
            if(File.Exists(logFilePath)) File.Delete(logFilePath);
        }
    }
}
