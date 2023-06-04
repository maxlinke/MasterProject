using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P.RatingFunctions {

    public abstract class RatingFunction {

        public virtual string Id => this.GetType().Name;

        public abstract float Evaluate (int playerIndex, G44PGameState gameState, int depth);

    }

}
