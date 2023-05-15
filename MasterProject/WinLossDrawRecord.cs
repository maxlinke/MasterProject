using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {
    
    public class WinLossDrawRecord {

        public string[] playerIds { get; set; }
        public int matchupSize { get; set; }
        public int[] totalWins { get; set; }
        public int[] totalLosses { get; set; }
        public int[] totalDraws { get; set; }
        public List<int>[] matchupWinners { get; set; } // TODO does this serialize as json?

        public WinLossDrawRecord (IReadOnlyList<string> playerIds, int playersPerGame) {
            this.playerIds = playerIds.ToArray();
            this.totalWins = new int[playerIds.Count];
            this.totalLosses = new int[playerIds.Count];
            this.totalDraws = new int[playerIds.Count];
            this.matchupSize = playersPerGame;
            this.matchupWinners = new List<int>[GetPossibleMatchupCount(playerIds.Count, playersPerGame)];
            for (int i = 0; i < matchupWinners.Length; i++) {
                matchupWinners[i] = new List<int>();
            }
        }

        public int GetTotalWins (string player) {
            return totalWins[Array.IndexOf(playerIds, player)];
        }

        public int GetTotalLosses (string player) {
            return totalLosses[Array.IndexOf(playerIds, player)];
        }

        public int GetTotalDraws (string player) {
            return totalDraws[Array.IndexOf(playerIds, player)];
        }

        public IEnumerable<(IReadOnlyList<string> participants, int winnerIndex)> GetMatchupResultsForPlayer (string player) {
            // TODO there's probably a smarter way than iterating over all possible matchups
            // but is it it really neccessary? probably not. 
            for (int i = 0; i < matchupWinners.Length; i++) {
                var matchup = GetMatchupFromIndex(i);
                if (matchup.Contains(player)) {
                    foreach (var winner in matchupWinners[i]) {
                        yield return (matchup, winner);
                    }
                }
            }
        }

        public const int DRAW = -1;

        public void RecordWin (IReadOnlyList<string> keys, int winnerIndex) {
            matchupWinners[GetMatchupIndex(keys)].Add(winnerIndex);
            for(int i=0; i<keys.Count; i++){
                var playerId = keys[i];
                var playerIndex = Array.IndexOf(playerIds, playerId);
                if (i == winnerIndex) {
                    totalWins[playerIndex]++;
                } else {
                    totalLosses[playerIndex]++;
                }
            }
        }

        public void RecordDraw (IReadOnlyList<string> keys) {
            matchupWinners[GetMatchupIndex(keys)].Add(DRAW);
            for (int i = 0; i < keys.Count; i++) {
                var playerId = keys[i];
                var playerIndex = Array.IndexOf(playerIds, playerId);
                totalDraws[playerIndex]++;
            }
        }

        public static int GetPossibleMatchupCount (int numberOfPlayers, int playersPerGame) {
            var output = 1;
            for (int i = 0; i < playersPerGame; i++) {
                output *= numberOfPlayers;
            }
            return output;
        }

        //public static IReadOnlyList<IReadOnlyList<string>> GetPossibleMatchups (IReadOnlyList<string> players, int playersPerGame) {
        //    var temp = new WinLossDrawRecord(players, playersPerGame);
        //    var output = new string[temp.matchupWinners.Length][];
        //    for (int i = 0; i < output.Length; i++) {
        //        output[i] = temp.GetMatchupFromIndex(i).ToArray();
        //    }
        //    return output;
        //}

        public int GetMatchupIndex (IReadOnlyList<string> keys) {
            if (keys.Count != matchupSize) {
                throw new ArgumentException($"Matchup must consist of {matchupSize} players, but consisted of {keys.Count} instead!");
            }
            var output = 0;
            foreach (var key in keys) {
                var index = Array.IndexOf(this.playerIds, key);
                if (index == -1) {
                    throw new ArgumentException($"\"{key}\" is not registered!");
                }
                output *= playerIds.Length;
                output += index;
            }
            return output;
        }
           
        public IReadOnlyList<string> GetMatchupFromIndex (int matchupIndex) {
            var keys = new List<string>();
            for (int i = 0; i < matchupSize; i++) {
                var playerIndex = matchupIndex % playerIds.Length;
                matchupIndex -= playerIndex;
                matchupIndex /= playerIds.Length;
                keys.Insert(0, playerIds[playerIndex]);
            }
            return keys;
        }

        public static WinLossDrawRecord Merge (WinLossDrawRecord a, WinLossDrawRecord b) {
            if (a.matchupSize != b.matchupSize) {
                throw new NotImplementedException("Matchup size must be the same when merging!");
            }
            var combinedPlayers = new List<string>();
            EnsureIdsRegistered(a.playerIds);
            EnsureIdsRegistered(b.playerIds);
            var output = new WinLossDrawRecord(combinedPlayers, a.matchupSize);
            CopyTotals(a);
            CopyTotals(b);
            CopyResults(a);
            CopyResults(b);
            return output;

            void EnsureIdsRegistered (IEnumerable<string> idsToRegister) {
                foreach (var id in idsToRegister) {
                    if (!combinedPlayers.Contains(id)) {
                        combinedPlayers.Add(id);
                    }
                }
            }

            void CopyTotals (WinLossDrawRecord src) {
                for (int i = 0; i < src.playerIds.Length; i++) {
                    var dstIndex = Array.IndexOf(output.playerIds, src.playerIds[i]);
                    output.totalWins[dstIndex] += src.totalWins[i];
                    output.totalLosses[dstIndex] += src.totalLosses[i];
                    output.totalDraws[dstIndex] += src.totalDraws[i];
                }
            }

            void CopyResults (WinLossDrawRecord src) {
                for (int i = 0; i < src.matchupWinners.Length; i++) {
                    if (src.matchupWinners[i].Count > 0) {
                        var players = src.GetMatchupFromIndex(i);
                        var outputIndex = output.GetMatchupIndex(players);
                        foreach (var winner in src.matchupWinners[i]) {
                            output.matchupWinners[outputIndex].Add(winner);
                        }
                    }
                }
            }
        }

    }

}
