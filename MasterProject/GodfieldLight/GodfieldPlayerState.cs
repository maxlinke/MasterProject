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
                cards = (cloneDeck ? new List<Card>(this.cards) : null),
                cardIds = (cloneDeck ? new List<string>(this.cardIds) : null)
            };
        }

    }

}
