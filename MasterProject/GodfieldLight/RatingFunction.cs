using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight {

    public abstract class RatingFunction {

        public virtual string Id => this.GetType().Name;

        public abstract float RateMove (GodfieldGameState gameState, GodfieldMove move, IReadOnlyList<int> otherPlayersInOrderOfMostHealthToLeast);
    
    }

}
