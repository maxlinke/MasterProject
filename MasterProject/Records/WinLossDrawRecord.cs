using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.Records {

    public class WinLossDrawRecord {

        public class MatchupRecord {

            public const char WIN = 'W';
            public const char LOSS = 'L';
            public const char DRAW = 'D';

            public string[] playerIds { get; set; }
            public List<string> gameResults { get; set; }       // not optional. strings because of the json export. makes everything easier to parse and is shorter than another array which would have commas
            public List<GameRecord> gameRecords { get; set; }   // optional (full info on the entire game)

            public override bool Equals (object? obj) {
                return obj is MatchupRecord other
                    && CompareCollection(this.playerIds, other.playerIds)
                    && CompareCollection(this.gameResults, other.gameResults)
                    && CompareCollection(this.gameRecords, other.gameRecords)
                ;
            }

            public override int GetHashCode () {
                return HashCode.Combine(playerIds, gameResults, gameRecords);
            }

            public MatchupRecord Clone () {
                var output = new MatchupRecord();
                output.playerIds = this.playerIds.Select((id) => id).ToArray(); // simple string duplication
                output.gameResults = new List<string>(this.gameResults);        // simple string duplication
                output.gameRecords = new List<GameRecord>();                    // inefficient but simple cloning via serialization
                foreach(var gr in this.gameRecords){
                    var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(gr);
                    var clone = System.Text.Json.JsonSerializer.Deserialize<GameRecord>(json);
                    if (clone != default) {
                        output.gameRecords.Add(clone);
                    }
                };
                return output;
            }

        }

        public string[] playerIds { get; set; }
        public float[] elo { get; set; }
        public int matchupSize { get; set; }
        public int[] totalWins { get; set; }
        public int[] totalLosses { get; set; }
        public int[] totalDraws { get; set; }
        public MatchupRecord[] matchupRecords { get; set; }

        public int GetMatchupCount () => matchupRecords.Length;
        public int GetNumberOfMatchesPlayedInMatchup (int matchupIndex) => matchupRecords[matchupIndex].gameResults.Count;
        public int GetNumberOfMatchesPlayedInMatchup (IReadOnlyList<string> players) => GetNumberOfMatchesPlayedInMatchup(GetMatchupIndex(players));
        public int GetNumberOfMatchesPlayedInTotal () {
            var output = 0;
            foreach (var record in matchupRecords) {
                output += record.gameResults.Count;
            }
            return output;
        }

        public override bool Equals (object? obj) {
            return obj is WinLossDrawRecord other
                && (this.matchupSize == other.matchupSize)
                && CompareCollection(this.playerIds, other.playerIds)
                && CompareCollection(this.totalWins, other.totalWins)
                && CompareCollection(this.totalLosses, other.totalLosses)
                && CompareCollection(this.totalDraws, other.totalDraws)
                && CompareCollection(this.matchupRecords, other.matchupRecords)
            ;
        }

        public override int GetHashCode () {
            return HashCode.Combine(playerIds, matchupSize, totalWins, totalLosses, totalDraws, matchupRecords);
        }

        public void CalculateElo () {
            Console.WriteLine("TODO calculate elo");  // TODO
        }

        public static WinLossDrawRecord Empty (int playersPerGame) => New(new string[0], playersPerGame);

        public static WinLossDrawRecord New (IReadOnlyList<string> playerIds, int playersPerGame) {
            var output = new WinLossDrawRecord();
            output.playerIds = playerIds.ToArray();
            output.elo = new float[playerIds.Count];
            output.totalWins = new int[playerIds.Count];
            output.totalLosses = new int[playerIds.Count];
            output.totalDraws = new int[playerIds.Count];
            output.matchupSize = playersPerGame;
            var matchupCount = GetPossibleMatchupCount(playerIds.Count, playersPerGame);
            output.matchupRecords = new MatchupRecord[matchupCount];
            for (int i = 0; i < matchupCount; i++) {
                output.matchupRecords[i] = new MatchupRecord();
                var newMatchupRecord = output.matchupRecords[i];
                newMatchupRecord.playerIds = output.GetMatchupFromIndex(i).ToArray();
                newMatchupRecord.gameResults = new List<string>();
                newMatchupRecord.gameRecords = new List<GameRecord>();
            }
            return output;
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

        public IEnumerable<MatchupRecord> GetMatchupRecordsForPlayer (string player) {
            // TODO there's probably a smarter way than iterating over all possible matchups
            // but is it it really neccessary? probably not. 
            for (int i = 0; i < matchupRecords.Length; i++) {
                var matchup = GetMatchupFromIndex(i);
                if (matchup.Contains(player)) {
                    yield return matchupRecords[i];
                }
            }
        }

        public void RecordResult (IReadOnlyList<string> keys, GameState gameState) {
            var record = matchupRecords[GetMatchupIndex(keys)];
            var result = new char[keys.Count];
            for(int i=0; i<keys.Count; i++){
                var playerId = keys[i];
                var playerIndex = Array.IndexOf(playerIds, playerId);
                if (gameState.GetPlayerHasLost(i)) {
                    result[i] = MatchupRecord.LOSS;
                    totalLosses[playerIndex]++;
                } else if (gameState.GetPlayerHasDrawn(i) || !gameState.GameOver) {
                    result[i] = MatchupRecord.DRAW;
                    totalDraws[playerIndex]++;
                } else if (gameState.GetPlayerHasWon(i)) {
                    result[i] = MatchupRecord.WIN;
                    totalWins[playerIndex]++;
                } else {
                    throw new NotSupportedException($"Unable to determine what to record for player \"{playerId}\" after game!");
                }
            }
            record.gameResults.Add(new string(result));
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

        public WinLossDrawRecord Remove (string playerId) {
            return Remove(new string[] { playerId });
        }

        public WinLossDrawRecord Remove (IEnumerable<string> playerIds) {
            var remainingIds = new List<string>(this.playerIds);
            remainingIds.RemoveAll(id => playerIds.Contains(id));
            var output = WinLossDrawRecord.New(remainingIds, this.matchupSize);
            for (int i = 0; i < output.matchupRecords.Length; i++) {
                var participants = output.GetMatchupFromIndex(i);
                var origI = this.GetMatchupIndex(participants);
                output.matchupRecords[i] = this.matchupRecords[origI].Clone();
            }
            return output;
        }

        public static WinLossDrawRecord Merge (WinLossDrawRecord a, WinLossDrawRecord b) {
            if (a.matchupSize != b.matchupSize) {
                throw new NotImplementedException("Matchup size must be the same when merging!");
            }
            var combinedPlayers = new List<string>();
            EnsureIdsRegistered(a.playerIds);
            EnsureIdsRegistered(b.playerIds);
            var output = New(combinedPlayers, a.matchupSize);
            CopyTotals(a);
            CopyTotals(b);
            CopyRecords(a);
            CopyRecords(b);
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

            void CopyRecords (WinLossDrawRecord src) {
                for (int i = 0; i < src.matchupRecords.Length; i++) {
                    var srcRecordClone = src.matchupRecords[i].Clone();
                    if (srcRecordClone.gameResults.Count > 0) {
                        var players = src.GetMatchupFromIndex(i);
                        var outputIndex = output.GetMatchupIndex(players);
                        output.matchupRecords[outputIndex].gameResults.AddRange(srcRecordClone.gameResults);
                        output.matchupRecords[outputIndex].gameRecords.AddRange(srcRecordClone.gameRecords);
                    }
                }
            }
        }

        public string ToJson () {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }

        public byte[] ToJsonBytes () {
            return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(this);
        }

        public static WinLossDrawRecord FromJson (string json) {
            var output = System.Text.Json.JsonSerializer.Deserialize<WinLossDrawRecord>(json);
            if (output == null) {
                throw new InvalidDataException("Couldn't deserialize record!");
            }
            return output;
        }

        public static WinLossDrawRecord FromJsonBytes (byte[] bytes) {
            return FromJson(System.Text.Encoding.UTF8.GetString(bytes));
        }

        protected static bool CompareCollection<T> (IReadOnlyList<T> a, IReadOnlyList<T> b, System.Func<T, T, bool> compareElement = null) {
            if ((a == null) != (b == null)) {
                return false;
            }
            if (a.Count != b.Count) {
                return false;
            }
            if (compareElement == null) {
                compareElement = (a, b) => a.Equals(b);
            }
            for (int i = 0; i < a.Count; i++) {
                if (!compareElement(a[i], b[i])) {
                    return false;
                }
            }
            return true;
        }

    }

}
