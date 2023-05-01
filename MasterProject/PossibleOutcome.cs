using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public class PossibleOutcome<T> {

        public float Probability { get; set; }

        public T? Outcome { get; set; }

    }

}
