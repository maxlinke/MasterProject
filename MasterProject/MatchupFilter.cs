using System.Collections.Generic;

namespace MasterProject {

    public static class MatchupFilter {

        private class DynamicFilter : IMatchupFilter {

            private readonly System.Func<IReadOnlyList<string>, bool> preventMatchup;

            public DynamicFilter (System.Func<IReadOnlyList<string>, bool> preventMatchup) {
                this.preventMatchup = preventMatchup;
            }

            bool IMatchupFilter.PreventMatchup (IReadOnlyList<string> agentIds) {
                return preventMatchup(agentIds);
            }

        }

        public static IMatchupFilter AllowAllMatchups => allowAll;
        private static readonly DynamicFilter allowAll = new DynamicFilter(_ => false);

        public static IMatchupFilter PreventMirrorMatches => preventMirrorMatches;
        private static readonly DynamicFilter preventMirrorMatches = new DynamicFilter(agentIds => {
            for (int i = 1; i < agentIds.Count; i++) {
                if (agentIds[i - 1] != agentIds[i]) {
                    return false;
                }
            }
            return true;
        });

        public static IMatchupFilter PreventAnyDuplicateAgents => preventAnyDuplicates;
        private static readonly DynamicFilter preventAnyDuplicates = new DynamicFilter(agentIds => {
            for (int i = 0; i < agentIds.Count; i++) {
                for (int j = i + 1; j < agentIds.Count; j++) {
                    if (agentIds[i] == agentIds[j]) {
                        return true;
                    }
                }
            }
            return false;
        });

        public static IMatchupFilter EnsureAgentIsContainedOncePerMatchup (Agent agent) {
            return new DynamicFilter(agentIds => {
                var count = 0;
                foreach (var id in agentIds) {
                    if (id == agent.Id) {
                        count++;
                    }
                }
                return count == 1;
            });
        }

    }

}
