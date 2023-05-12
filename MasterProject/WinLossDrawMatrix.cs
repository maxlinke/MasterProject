using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    // make exportable as json
    // also make something up for csv
    // unless c# already has something?
    // c# has Microsoft.VisualBasic.FileIO.TextFieldParser but that works on files
    // which is fine-ish?
    // but i'd much rather feed it a string, just like the json thing
    // on the other hand, do i really need it?
    
    public class WinLossDrawRecord {

        // TODO this needs to preserve the order
        // i.e. the winner was the first player OR the winner was the second player
        // then i should maybe just save base-playerstates in order?

        //public Dictionary<string, Dictionary<string, int>> wins   { get; set; } = new();
        //public Dictionary<string, Dictionary<string, int>> losses { get; set; } = new();
        //public Dictionary<string, Dictionary<string, int>> draws  { get; set; } = new();

        public void RecordWin (string winner, params string[] losers) {
            //if (!wins.ContainsKey(winner)) {
            //    wins.Add(winner, new Dictionary<string, int>());
            //}
            //foreach (var loser in losers) {
            //    if (!wins[winner].ContainsKey(loser)) {
            //        wins[winner].Add(loser, 0);
            //    }
            //    if (!losses.ContainsKey(loser)) {
            //        losses.Add(loser, new Dictionary<string, int>());
            //    }
            //    if (!losses[loser].ContainsKey(winner)) {
            //        losses[loser].Add(winner, 0);
            //    }
            //    wins[winner][loser]++;
            //    losses[loser][winner]++;
            //}
        }

        public void RecordDraw (params string[] participants) {
            //foreach(parti
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
