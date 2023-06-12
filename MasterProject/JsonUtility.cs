using System.Text.Json;

namespace MasterProject {
    
    public static class JsonUtility {

        public static string ToJson (object obj, bool prettyPrint = false) {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = prettyPrint });
        }

        public static byte[] ToJsonBytes (object obj, bool prettyPrint = false) {
            return JsonSerializer.SerializeToUtf8Bytes(obj, new JsonSerializerOptions { WriteIndented = prettyPrint });
        }

        public static T FromJson<T> (string json) {
            return JsonSerializer.Deserialize<T>(json);
        }

        public static T FromJsonBytes<T> (byte[] json) {
            return JsonSerializer.Deserialize<T>(json);
        }

    }

}
