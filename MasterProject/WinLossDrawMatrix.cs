using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {
    
    public class WinLossDrawRecord {

        public string[] playerIds { get; set; }
        public int dimensionality { get; set; }
        public List<int>[] matchupWinners { get; set; } // TODO does this serialize as json?

        public WinLossDrawRecord (IReadOnlyList<string> playerIds, int playersPerGame) {
            this.playerIds = playerIds.ToArray();
            this.dimensionality = playersPerGame;
            this.matchupWinners = new List<int>[GetPossibleMatchupCount(playerIds.Count, playersPerGame)];
            for (int i = 0; i < matchupWinners.Length; i++) {
                matchupWinners[i] = new List<int>();
            }
        }

        public const int DRAW = -1;

        public void RecordWin (IReadOnlyList<string> keys, int winnerIndex) {
            matchupWinners[GetMatchupIndex(keys)].Add(winnerIndex);
        }

        public void RecordDraw (IReadOnlyList<string> keys) {
            RecordWin(keys, DRAW);
        }

        public static int GetPossibleMatchupCount (int numberOfPlayers, int playersPerGame) {
            var output = 1;
            for (int i = 0; i < playersPerGame; i++) {
                output *= numberOfPlayers;
            }
            return output;
        }

        // TODO test all this matchup math
        public int GetMatchupIndex (IReadOnlyList<string> keys) {
            if (keys.Count != dimensionality) {
                throw new ArgumentException($"Matchup must consist of {dimensionality} players, but consisted of {keys.Count} instead!");
            }
            var output = 0;
            foreach (var key in keys) {
                var index = Array.IndexOf(this.playerIds, key);
                if (index == -1) {
                    throw new ArgumentException($"\"{key}\" is not registered!");
                }
                output *= dimensionality;
                output += index;
            }
            return output;
        }
           
        // TODO check this too
        public IReadOnlyList<string> GetMatchupFromIndex (int matchupIndex) {
            var keys = new List<string>();
            for (int i = 0; i < dimensionality; i++) {
                var playerIndex = matchupIndex % dimensionality;
                matchupIndex -= playerIndex;
                matchupIndex /= dimensionality;
            }
            return keys;
        }

        //public static WinLossDrawRecord Merge (WinLossDrawRecord a, WinLossDrawRecord b) {
        //    a ??= new();
        //    b ??= new();
        //    var output = new WinLossDrawRecord();
        //    AddMainDict(a.wins, output.wins);
        //    AddMainDict(b.wins, output.wins);
        //    AddMainDict(a.losses, output.losses);
        //    AddMainDict(b.losses, output.losses);
        //    AddMainDict(a.draws, output.draws);
        //    AddMainDict(b.draws, output.draws);
        //    return output;

        //    void AddMainDict (Dictionary<string, Dictionary<string, int>> src, Dictionary<string, Dictionary<string, int>> dst) {
        //        foreach (var mainKvp in src) {
        //            var key = mainKvp.Key;
        //            dst[key] = dst[key] ?? new Dictionary<string, int>();
        //            AddSubDict(src[key], dst[key]);
        //        }

        //        void AddSubDict (Dictionary<string, int> src, Dictionary<string, int> dst) {
        //            foreach (var subKvp in src) {
        //                var key = subKvp.Key;
        //                if (!dst.ContainsKey(key)) {
        //                    dst[key] = 0;
        //                }
        //                dst[key] += subKvp.Value;
        //            }
        //        }
        //    }
        //}

    }

}
