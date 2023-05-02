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
        Console.WriteLine("Main started!");
        new Thread(Countdown).Start(3);
        Console.WriteLine("Main finished!");
    }

    static void Countdown (object? data) {
        data ??= 0;
        int seconds = (int)data;
        Console.WriteLine("Countdown started!");
        while (seconds > 0) {
            Console.WriteLine($"{seconds} seconds left!");
            seconds--;
            Thread.Sleep(1000);
        }
        Console.WriteLine("Countdown finished!");
    }

    // TODO figure out the await with timeout thing in here. 
    // convert the countdown thing into a task or whatever
    // https://stackoverflow.com/questions/4238345/asynchronously-wait-for-taskt-to-complete-with-timeout
    // https://devblogs.microsoft.com/pfxteam/crafting-a-task-timeoutafter-method/
    // https://stackoverflow.com/questions/35645899/awaiting-task-with-timeout
    // https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/await

    // i guess that means game.run needs to be a task too? or at least async void?
    // but those can't be abstract, i think...
    // also how do exceptions work there?
    // i mean, i can also just ditch the timeout thing
    // not like it really matters

}

