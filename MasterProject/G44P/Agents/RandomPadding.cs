using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.G44P.Agents {

    public class RandomPadding : RandomAgent {

        public override Agent Clone () {
            return new RandomPadding();
        }

    }

}
