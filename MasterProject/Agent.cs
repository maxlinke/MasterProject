using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    // TODO agentrecord
    // for rating, number of games played and all that
    // use a single db-file for all agents?
    // pros: easy loading in web visulization
    // pros: only one read and all agents are in memory
    // cons: probably bad in terms of writing time
    // cons: easy to corrupt? (version control at least prevents really bad stuff)

    public abstract class Agent {

        public virtual string Id => $"{this.GetType().FullName}";

        private static readonly Random globalRng = new();

        public static int GetRandomMoveIndex<TMove> (IReadOnlyList<TMove> moves) {
            return globalRng.Next(moves.Count);
        }

    }

    public abstract class Agent<TGame, TMove> : Agent {

        protected readonly Random rng = new();

        public abstract int GetMoveIndex (TGame game, IReadOnlyList<TMove> moves);

    }

}
