using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight.Cards {

    public abstract class CardData {

        static CardData () {
            var map = new Dictionary<Card, CardData> ();
            var total = 0;
            foreach (var data in allCardData) {
                total += data.occurrences;
                map.Add(data.card, data);
            }
            occurrenceTotal = total;
            cardDataMap = map;
        }

        public static Card GetRandomCard () {
            var t = rng.Next(occurrenceTotal);
            for (int i = 0; i < allCardData.Count; i++) {
                t -= allCardData[i].occurrences;
                if (i <= 0) {
                    return allCardData[i].card;
                }
            }
            return allCardData[allCardData.Count - 1].card;
        }

        private static readonly System.Random rng = new Random();

        private static readonly int occurrenceTotal;

        public static readonly IReadOnlyDictionary<Card, CardData> cardDataMap;

        public static readonly IReadOnlyList<CardData> allCardData = new CardData[]{
            // TODO
        };

        public abstract Card card { get; }

        public abstract int occurrences { get; }
        public abstract bool usableInOwnTurn { get; }
        public abstract bool usableWhileDefending { get; }  // i think this needs to be a get-function because the "attack" has some complexity too (elements, bounce, reflect, ...)

        // how do i do stuff like the bounce sword and reflect sword?
        // abstraction probably?

        private CardData () { }


    }

}
