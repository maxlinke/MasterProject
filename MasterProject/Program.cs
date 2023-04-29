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
        Console.WriteLine(GetProjectPath());
        //Console.WriteLine("Hello, world!");
        //var input = Console.ReadLine();
        //Console.WriteLine($"You said \"{input}\"");
        //IReadOnlyList<int> ints = new List<int>() { 1, 2, 3 };
        //ints[2] = 5;

        //var gs1 = new TestGS(3);
        //var gs2 = new TestGS(gs1);

        var objs = new List<object>() {
            new IntClass(7),
            new StringClass("lmao"),
            //new ComplexClass(new Class2(3, "hehe"))
            new ComplexClass(new Class2(3){myString = "hehe" })
            //new Class2(),
            //new Class2() { myInts = new int[] { 3, 5, 7 } }
        };

        var json = new List<string>();

        for (int i = 0; i < objs.Count; i++) {
            json.Add(JsonSerializer.Serialize(objs[i]));
            Console.WriteLine($"{i}: ");
            Console.WriteLine(json[i]);
        }

        Console.WriteLine();

        var clones = new object[objs.Count];
        clones[0] = JsonSerializer.Deserialize<IntClass>(json[0]);
        clones[1] = JsonSerializer.Deserialize<StringClass>(json[1]);
        clones[2] = JsonSerializer.Deserialize<ComplexClass>(json[2]);

        for (int i = 0; i < clones.Length; i++) {
            Console.WriteLine($"{i}: ");
            Console.WriteLine(clones[i]);
        }

        //var intThing = new IntClass(4);
        //Console.WriteLine(intThing);
        //var json = JsonSerializer.Serialize(intThing);
        //Console.WriteLine(json);
        //var clone = JsonSerializer.Deserialize<IntClass>(json);
        //Console.WriteLine(clone);

    }



    static string ListElements<T> (IEnumerable<T> objs) {
        var sb = new System.Text.StringBuilder();
        foreach (var obj in objs) {
            sb.Append($"{obj}, ");
        }
        if (sb.Length > 0) {
            sb.Remove(sb.Length - 2, 2);
        }
        return sb.ToString();
    }

    public abstract class Class1<T> {

        //[JsonConstructor]
        //private Class1 () { }

        public Class1(T myField) {
            this.myField = myField;
        }

        [JsonInclude] public T myField;

        public override string ToString () {
            return myField == null ? "<null>" : myField.ToString();
        }

    }

    public class IntClass : Class1<int> {
        public IntClass (int myField) : base(myField) { }
    }

    public class StringClass : Class1<string> {
        public StringClass (string myField) : base(myField) { }
    }

    public class ComplexClass : Class1<Class2> {
        public ComplexClass (Class2 myField) : base(myField) { }
    }

    public class Class2 {
        public Class2 (int myInt /* , string myString */ ) {
            this.myInt = myInt;
            //this.myString = myString;
        }
        [JsonInclude] public int myInt;
        [JsonInclude] public string myString;
        public override string ToString () {
            return $"{myInt}, {myString}";
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

