namespace MasterProject {

    public class MoveRecord {

        public object[]? AvailableMoves { get; set; }
        public int? ChosenMoveIndex { get; set; }
        public long? MoveChoiceDurationMillis { get; set; }
        public bool? MoveChoiceTimedOut { get; set; }

    }

}
