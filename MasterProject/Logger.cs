using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace MasterProject {

    public static class Logger {

        static bool fileStreamInitialized = false;
        static FileStream fs;
        static StreamWriter sw;

        public static bool logToDisk { get; set; }

        public static void Log (object message) {
            if (logToDisk && !fileStreamInitialized) {
                var logsDirectory = Path.Combine(Program.GetProjectPath(), "Logs");
                if (!Directory.Exists(logsDirectory)) {
                    Directory.CreateDirectory(logsDirectory);
                }
                var filePath = Path.Combine(logsDirectory, $"Log_{System.DateTime.Now.Ticks}.txt");
                fs = new FileStream(filePath, FileMode.Create);
                sw = new StreamWriter(fs);
                sw.AutoFlush = true;
                fileStreamInitialized = true;
            }
            var msg = message.ToString();
            Console.WriteLine(msg);
            if (logToDisk) {
                sw.WriteLine(msg);
            }
        }

        public static void Flush () {
            if (fileStreamInitialized) {
                sw.Flush();
                fs.Flush();
            }
        }

    }

}
