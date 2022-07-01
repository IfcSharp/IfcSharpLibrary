using NetSystem = System;
using System.IO;

namespace ifc
{
    //TODO: implement filter for output
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

        private const bool OutputToConsole = true;
        private static string logFilePath = "ifcsharp.log";
        public static void Add(string msg, Level level) {
            string log = $"{NetSystem.DateTime.Now:O}[{level}]: {msg}";
            if(OutputToConsole) NetSystem.Console.WriteLine(log);
            try {
                using (StreamWriter sw = new StreamWriter(path: logFilePath, append: true)) {
                    sw.WriteLine(log);
                    sw.Close();
                }
            }
            catch(NetSystem.Exception e)
            {
                NetSystem.Console.WriteLine(e);
            }
        }

        public static void SetLogfilePath(string path) {
            logFilePath = path;
        }
        public static void Reset() {
            if(File.Exists(logFilePath)) File.Delete(logFilePath);
        }
    }
}
