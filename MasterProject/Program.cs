// See https://aka.ms/new-console-template for more information

using MasterProject;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Program {

    public static string GetProjectPath () {
        var binPath = Environment.CurrentDirectory;
        if (binPath.Contains("\\bin\\Debug\\")) {
            return binPath.Substring(0, binPath.LastIndexOf("\\bin\\Debug\\") + 1);
        }
        return binPath;
    }

    public static void Main (string[] args) {
        DoSyncAsyncTest().GetAwaiter().GetResult();
    }

    static Random rng = new();

    static async Task DoSyncAsyncTest () {
        var sw = new System.Diagnostics.Stopwatch();
        var itCount = 10000;
        for (int i = 0; i < 5; i++) {
            sw.Restart();
            var synced = GetRngThingSynced(itCount);
            sw.Stop();
            Console.WriteLine($"Synced got value {synced} in {sw.ElapsedMilliseconds}ms");
            sw.Restart();
            var asynced = await GetRngThingAsync(itCount);
            sw.Stop();
            Console.WriteLine($"Async got value {asynced} in {sw.ElapsedMilliseconds}ms");
        }
    }

    static int GetRngThingSynced (int count) {
        count = Math.Max(0, count);
        var output = 0;
        for (int i = 0; i < count; i++) {
            output ^= rng.Next();
        }
        return output;
    }

    static async Task<int> GetRngThingAsync (int count) {
        count = Math.Max(0, count);
        var output = 0;
        for (int i = 0; i < count; i++) {
            output ^= await GetSingleRngValueAsTask();
        }
        return output;
    }

    static async Task<int> GetSingleRngValueAsTask () {
        return await Task.Run(() => rng.Next());
    }

}

