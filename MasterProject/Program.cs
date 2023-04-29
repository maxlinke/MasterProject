// See https://aka.ms/new-console-template for more information

using MasterProject;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Program {

    public static void Main (string[] args) {
        //Console.WriteLine("Hello, world!");
        //var input = Console.ReadLine();
        //Console.WriteLine($"You said \"{input}\"");
        //IReadOnlyList<int> ints = new List<int>() { 1, 2, 3 };
        //ints[2] = 5;

        var gs1 = new TestGS(3);
        var gs2 = new TestGS(gs1);

        var objs = new List<object>() {
            new Class1(),
            new Class1(new int[] { 2, 4, 6 }),
            //new Class2(),
            //new Class2() { myInts = new int[] { 3, 5, 7 } }
        };

        var json = new List<string>();

        for(int i=0; i<objs.Count; i++) {
            json.Add(JsonSerializer.Serialize(objs[i]));
            Console.WriteLine($"{i}: ");
            Console.WriteLine(json[i]);
        }

        Console.WriteLine();

        var clones = new List<object>();

        for(int i=0; i<json.Count; i++) {
            clones.Add(JsonSerializer.Deserialize<Class1>(json[i]));
            Console.WriteLine($"{i}: ");
            Console.WriteLine(clones[i].ToString());
        }
    }

    static string ListElements<T> (IEnumerable<T> objs) {
        var sb = new System.Text.StringBuilder();
        foreach(var obj in objs) {
            sb.Append($"{obj}, ");
        }
        if(sb.Length > 0) {
            sb.Remove(sb.Length - 2, 2);
        }
        return sb.ToString();
    }

    public class Class1 {

        public Class1 () {
            myInts = null;
        }

        public Class1 (int[] src) {
            myInts = src;
        }

        [JsonInclude]
        public int[]? myInts { get; private set; }

        public override string ToString () {
            return myInts == null ? "<null>" : ListElements(myInts);
        }

    }

    class TestGS : GameState<TestGS, TestMove> {
        public TestGS (int playerCount) : base(playerCount) {
        }

        public TestGS (GameState<TestGS, TestMove> source) : base(source) {
        }
    }

    class TestMove : Move <TestGS, TestMove> {

    }

}

