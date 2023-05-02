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

    }

    public abstract class Agent<TGame, TMove> : Agent {

        public abstract void OnGameStarted (TGame game);

        public abstract int GetMoveIndex (IReadOnlyList<TMove> moves);

    }

}
