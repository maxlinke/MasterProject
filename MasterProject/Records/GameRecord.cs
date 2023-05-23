namespace MasterProject.Records {

    public class GameRecord {

        public string? GameType { get; set; }
        public string? GameId { get; set; }
        public bool? Completed { get; set; }
        public long? Timestamp { get; set; }
        public string[]? AgentIds { get; set; }
        public GameState[]? GameStates { get; set; }
        public MoveRecord[]? Moves { get; set; }

        public override bool Equals (object? obj) {
            return obj is GameRecord record &&
                   GameType == record.GameType &&
                   GameId == record.GameId &&
                   Completed == record.Completed &&
                   Timestamp == record.Timestamp &&
                   //EqualityComparer<string[]?>.Default.Equals(AgentIds, record.AgentIds) &&
                   //EqualityComparer<GameState[]?>.Default.Equals(GameStates, record.GameStates) &&
                   //EqualityComparer<MoveRecord[]?>.Default.Equals(Moves, record.Moves);
                   obj.GetHashCode() == this.GetHashCode();
        }

        public override int GetHashCode () {
            return HashCode.Combine(GameType, GameId, Completed, Timestamp, AgentIds, GameStates, Moves);
        }
    }

}
