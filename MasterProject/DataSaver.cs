using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MasterProject {

    public static class DataSaver {

        private class QueuedData {
            public string path;
            public byte[] bytes;
        }

        private static readonly Queue<QueuedData> queuedData = new Queue<QueuedData>();

        public static void SaveInProject (string path, byte[] bytes) {
            lock (queuedData) {
                queuedData.Enqueue(new QueuedData { path = $"{Program.GetProjectPath()}\\{path}", bytes = bytes });
            }
        }

        public static void Flush () {
            lock (queuedData) {
                while (queuedData.Count > 0) {
                    var data = queuedData.Dequeue();
                    var directoryPath = Path.GetDirectoryName(data.path);
                    if (!Directory.Exists(directoryPath)) {
                        Directory.CreateDirectory(directoryPath);
                    }
                    File.WriteAllBytes(data.path, data.bytes);
                }
            }
        }

    }

}
