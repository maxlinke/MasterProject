namespace MasterProject {

    public class GameRecord {

        public string? GameType { get; set; }
        public string? GameId { get; set; }
        public long? Timestamp { get; set; }
        public string[]? AgentIds { get; set; }
        public GameState[]? GameStates { get; set; }
        public int[]? MoveDurationsMillis { get; set; }

    }

}
