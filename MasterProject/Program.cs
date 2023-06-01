// See https://aka.ms/new-console-template for more information

using MasterProject;
using MasterProject.TicTacToe;
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

        TestG44P();

        DataSaver.Flush();
    }

    static void TestG44P () {
        //var gs = new G44PGameState();
        //const int testPlayerIndex = 0;
        //var otherPlayerIndex = (testPlayerIndex + 1) % G44PGameState.PLAYER_COUNT;
        //gs.Initialize(new string[] { "Jim" });
        //foreach (var fieldIndex in G44PGameState.PlayerHomeRows[testPlayerIndex]) {
        //    gs.PlacePiece(fieldIndex, testPlayerIndex);
        //}
        //foreach (var fieldIndex in G44PGameState.PlayerHomeRows[otherPlayerIndex]) {
        //    gs.PlacePiece(fieldIndex, otherPlayerIndex);
        //}
        //for (int i = 0; i < 10; i++) {
        //    Console.WriteLine();
        //    Console.WriteLine(gs.ToPrintableString());
        //    gs.MovePieces(testPlayerIndex);
        //    gs.RecalculatePlayerRanksAndUpdateWinnerIfApplicable();
        //    Console.WriteLine();
        //    Console.WriteLine(gs.ToPrintableString());
        //    gs.MovePieces(otherPlayerIndex);
        //    gs.RecalculatePlayerRanksAndUpdateWinnerIfApplicable();
        //}
        DoTournament<G44PGame>(
            continueId: "",
            numberOfPlayersPerMatchup: G44PGameState.PLAYER_COUNT,
            numberOfGamesToPlay: 1,
            filter: MatchupFilter.AllowAllMatchups,
            agents: new Agent<G44PGame, G44PGameState, G44PMove>[] {
                new MasterProject.G44P.Agents.RandomAgent()
            },
            saveResult: false,
            onBeforeRun: (tournament) => {
                tournament.AllowedGameConsoleOutputs = Game.ConsoleOutputs.Everything;
            }
        );
    }

    static void DoTTTTournament () {
        //const string continueTournamentId = "MasterProject.TicTacToe.TTTGame_638204554117514045";
        const string continueTournamentId = "";
        const int numberOfGamesToPlay = 10;
        var filter = MatchupFilter.AllowAllMatchups;

        DoTournament<TTTGame>(
            continueId: continueTournamentId,
            numberOfPlayersPerMatchup: 2,
            numberOfGamesToPlay: numberOfGamesToPlay,
            filter: filter,
            agents: new Agent<TTTGame, TTTGameState, TTTMove>[]{
                new MasterProject.TicTacToe.Agents.RandomAgent(),
                new MasterProject.TicTacToe.Agents.RandomAgentWithLookAhead(),
                new MasterProject.TicTacToe.Agents.LineBuilder(),
                //new MasterProject.TicTacToe.Agents.ABWin(),
                //new MasterProject.TicTacToe.Agents.ABLose(),
                new MasterProject.TicTacToe.Agents.ABWinFast(),
                new MasterProject.TicTacToe.Agents.ABLoseFast(),
                new DilutedAgent<TTTGame, TTTGameState, TTTMove>(new MasterProject.TicTacToe.Agents.ABWinFast(), 0.5f),
                new DilutedAgent<TTTGame, TTTGameState, TTTMove>(new MasterProject.TicTacToe.Agents.ABWinFast(), 0.8f),
                new DilutedAgent<TTTGame, TTTGameState, TTTMove>(new MasterProject.TicTacToe.Agents.ABWinFast(), 0.9f),
            },
            saveResult: true
        );
    }

    static void DoTournament<TGame> (string continueId, int numberOfPlayersPerMatchup, int numberOfGamesToPlay, IMatchupFilter filter, IReadOnlyList<Agent> agents, bool saveResult, System.Action<Tournament<TGame>> onBeforeRun = null) where TGame : Game, new() {
        Tournament<TGame> tournament;
        if (Tournament.TryLoadWinLossDrawRecord(continueId, out var loadedRecord)) {
            tournament = Tournament<TGame>.Continue(loadedRecord);
            Console.WriteLine("Continuing!");
        } else {
            tournament = Tournament<TGame>.New(numberOfPlayersPerMatchup);
            Console.WriteLine("New!");
        }

        tournament.MaxNumberOfGamesToRunInParallel = 16;
        onBeforeRun?.Invoke(tournament);
        tournament.Run(agents, numberOfGamesToPlay, filter);
        if (saveResult) {
            tournament.SaveWinLossDrawRecord();
        }
    }

}

