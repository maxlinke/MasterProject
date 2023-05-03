namespace MasterProject {

    public class GameRecord {

        public string? GameType { get; set; }
        public string? GameId { get; set; }
        public bool? Completed { get; set; }
        public long? Timestamp { get; set; }
        public string[]? AgentIds { get; set; }
        public GameState[]? GameStates { get; set; }
        public MoveRecord[]? Moves { get; set; }

    }

}
