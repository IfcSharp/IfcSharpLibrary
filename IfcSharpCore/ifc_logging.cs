using NetSystem = System;
using System.IO;
using System.Linq;

namespace ifc
{
    public static class Log
    {
        public enum Level
        {
            Info,
            Debug,
            Warning,
            Error,
            Exception
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
