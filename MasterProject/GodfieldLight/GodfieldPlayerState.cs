using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight {

    public class GodfieldPlayerState : PlayerState {

        public int health { get; set; }
        public List<Cards.Card> cards { get; set; }

    }

}
