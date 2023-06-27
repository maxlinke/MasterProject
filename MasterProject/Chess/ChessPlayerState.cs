namespace MasterProject.Chess {
    
    public class ChessPlayerState : PlayerState {

        public bool IsInCheck { get; set; }
        //public bool HasCastled { get; set; }      // TODO implement the "special" rules later

        public ChessPlayerState Clone () => new ChessPlayerState() {
            HasWon = this.HasWon,
            HasLost = this.HasLost,
            HasDrawn = this.HasDrawn,
            IsInCheck = this.IsInCheck,
            // TODO others, if they come up
        };

    }

}
