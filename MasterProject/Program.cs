// See https://aka.ms/new-console-template for more information

using MasterProject;
using MasterProject.TicTacToe;
using MasterProject.TicTacToe.Agents;
using MasterProject.G44P;
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
        //DoTTTTournament();

        //var foo = new int[4];
        //Console.WriteLine(foo[3]);
        //var bar = new List<int>(4);
        //Console.WriteLine(bar[3]);

        TestG44P();

        DataSaver.Flush();
    }

    static void TestG44P () {
        var gs = new G44PGameState();
        const int testPlayerIndex = 0;
        var otherPlayerIndex = (testPlayerIndex + 1) % G44PGameState.PLAYER_COUNT;
        gs.Initialize(new string[] { "Jim" });
        foreach (var fieldIndex in G44PGameState.PlayerHomeRows[testPlayerIndex]) {
            gs.PlacePiece(fieldIndex, testPlayerIndex);
        }
        foreach (var fieldIndex in G44PGameState.PlayerHomeRows[otherPlayerIndex]) {
            gs.PlacePiece(fieldIndex, otherPlayerIndex);
        }
        for (int i = 0; i < 10; i++) {
            Console.WriteLine();
            Console.WriteLine(gs.ToPrintableString());
            gs.MovePieces(testPlayerIndex);
            gs.RecalculatePlayerRanksAndUpdateWinnerIfApplicable();
            Console.WriteLine();
            Console.WriteLine(gs.ToPrintableString());
            gs.MovePieces(otherPlayerIndex);
            gs.RecalculatePlayerRanksAndUpdateWinnerIfApplicable();
        }
    }

    static void DoTTTTournament () {
        //const string continueTournamentId = "MasterProject.TicTacToe.TTTGame_638204554117514045";
        const string continueTournamentId = "";
        const int numberOfGamesToPlay = 100;
        const bool playMirrorMatches = false;

        Tournament<TTTGame> tournament;
        if (Tournament.TryLoadWinLossDrawRecord(continueTournamentId, out var loadedRecord)) {
            tournament = Tournament<TTTGame>.Continue(loadedRecord);
            Console.WriteLine("Continuing!");
        } else {
            tournament = Tournament<TTTGame>.New(2);
            Console.WriteLine("New!");
        }

        tournament.MaxNumberOfGamesToRunInParallel = 16;
        tournament.Run(new Agent<TTTGame, TTTGameState, TTTMove>[]{
            new RandomAgent(),
            new RandomAgentWithLookAhead(),
            new LineBuilder(),
            //new ABWin(),
            //new ABLose(),
            new ABWinFast(),
            new ABLoseFast(),
            new DilutedAgent<TTTGame, TTTGameState, TTTMove>(new ABWinFast(), 0.5f),
            new DilutedAgent<TTTGame, TTTGameState, TTTMove>(new ABWinFast(), 0.8f),
            new DilutedAgent<TTTGame, TTTGameState, TTTMove>(new ABWinFast(), 0.9f),
        }, numberOfGamesToPlay, playMirrorMatches);
        //tournament.SaveWinLossDrawRecord();
    }

}

