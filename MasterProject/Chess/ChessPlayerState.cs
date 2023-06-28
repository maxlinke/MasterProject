namespace MasterProject.Chess {
    
    public class ChessPlayerState : PlayerState {

        public bool IsInCheck { get; set; }
        public bool HasCastled { get; set; }
        public int KingCoord { get; set; }
        public long AttackMap { get; set; }

        public ChessPlayerState Clone () => new ChessPlayerState() {
            HasWon = this.HasWon,
            HasLost = this.HasLost,
            HasDrawn = this.HasDrawn,
            IsInCheck = this.IsInCheck,
            HasCastled = this.HasCastled,
            KingCoord = this.KingCoord,
            AttackMap = this.AttackMap,
            // TODO others, if they come up
        };

    }

}
