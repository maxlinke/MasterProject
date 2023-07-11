using System.Text.Json.Serialization;

namespace MasterProject.GodfieldLight {

    public class GodfieldPlayerState : PlayerState {

        public int index { get; set; }

        public int health { get; set; }

        [JsonIgnore]
        public List<Card> cards { get; set; }

        public List<string> cardIds { get; set; }

        public GodfieldPlayerState Clone (bool cloneDeck) {
            return new GodfieldPlayerState() {
                HasWon = this.HasWon,
                HasDrawn = this.HasDrawn,
                HasLost = this.HasLost,
                index = this.index,
                health = this.health,
                cards = ((cloneDeck && cards != null) ? new List<Card>(this.cards) : null),
                cardIds = ((cloneDeck && cardIds != null) ? new List<string>(this.cardIds) : null)
            };
        }

        public static GodfieldPlayerState[] CreateProperArrayClone (GodfieldPlayerState[] src) {
            var output = new GodfieldPlayerState[src.Length];
            for (int i = 0; i < output.Length; i++) {
                output[i] = src[i].Clone(true);
            }
            return output;
        }

        public static int GetLivingPlayerCount (GodfieldPlayerState[] src) {
            var output = 0;
            for (int i = 0; i < src.Length; i++) {
                if (src[i].health > 0) {
                    output++;
                }
            }
            return output;
        }

    }

}
