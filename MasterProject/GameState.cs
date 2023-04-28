using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {
    public abstract class GameState {    // class or struct? can structs do inheritance? are abstract structs a thing?

        // with classes, each implementation would have to ensure its immutability
        // with structs, that's a given
        // BUT abstract structs aren't a thing and neither is inheritance
        // so... interfaces? that makes stuff immutable-ish

    }

    public abstract class GameState<T, U> 
        where T : Move<U, T> 
        where U : GameState<T, U> 
    {

        public abstract IReadOnlyList<T> GetPossibleMoves ();   // no parameters? this assumes a player...

        // getter for current player? that'll have to go into the typed thing too
        // the thing is, do i need all the typing? does it make anything easier?
        // when it works, it does make it so i don't have to cast from abstract stuff to concrete stuff all the time
        // but how much casting will there even be?
        // those generics up there are already pretty bad
        // maybe the player has to generate the moves? 

        // maybe have the gamestate be rollback-able
        // which means to get the entire progress of the game, you only need the final game state and roll back to the beginning
        // that would also make the min-maxing less garbage-y?

        // all actions within a gamestate should be easily rollback-able
        // so something like multiplying a number by zero and remembering that i multiplied it by zero will be pretty useless because i can't roll that back
        // but keeping a straight line of numbers all the way to the beginning also sounds bad?
        // hm, maybe it does make everything way more complicated and keeping a list of gamestates would be easier
        // how do players factor into this though?
        // i guess they also need states?
        // what do the actual players do then?
        // or are they merely agents
        // who know which playerstate they correspond to, but they ARE not that state
        // that sounds okay?

    }
}
