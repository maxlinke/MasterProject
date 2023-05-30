namespace MasterProject {

    public interface IMatchupFilter {

        public bool PreventMatchup (IReadOnlyList<string> agentIds);

    }

}
