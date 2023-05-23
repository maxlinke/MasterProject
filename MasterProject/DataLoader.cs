using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MasterProject {

    public static class DataLoader {

        public static bool TryLoadInProject (string path, out byte[] output) {
            if (!path.StartsWith(Program.GetProjectPath())) {
                path = Path.Combine(Program.GetProjectPath(), path);
            }
            if (!File.Exists(path)) {
                output = null;
                return false;
            }
            output = File.ReadAllBytes(path);
            return true;
        }

    }

}
