using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public static class AlphaBetaMinMax {

        // two variants kinda
        // or even more
        // there's the standard zero-sum minimax stuff for two players
        // where we only need one rating function and can simply negamax the stuff
        // and then there's the n-player variant
        // which CAN simply try to minimize the score of the maximizing player
        // but they could also try to maximize their own scores
        // also working all the probability in here will be... fun...
        // well, if i have a rating for something i can just weigh it by its probability?
        // but that breaks if i have an infinity rating for example. 
        // i.e. there's a 1% chance that something will lead to a win now (infinite reward)
        // and a 99% chance that it won't
        // does it make sense to go for that 1% move?
        // i don't know
        // maybe i should check that
        // by also making the weighingfunction dynamic...
        // or at least an enum
        // what variations are there? clamp infinities and don't?
        // also i could just not give an infinite rating to things
        // this'll be easier with machine learning agents that can just learn what rating to give for the given baseline function that sorts the moves

        public static int GetBestMoveIndex<TGameState, TMove> (GameState<TGameState, TMove> gameState)
            where TGameState : GameState<TGameState, TMove>
        {
            // TODO start out with a simple assumption that it's deterministic and two player zero sum
            // throw notimplementedexceptions where that assumption breaks down
            return 0;
        }

	}

}
