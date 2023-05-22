'use strict';

function processData (input) {
    // TODO unpack the compact data
    const output = {};
    output.players = input.playerIds.map((id, i) => { return { 
        id: id,
        origIndex: i,
        totalWins: input.totalWins[i],
        totalLosses: input.totalLosses[i],
        totalDraws: input.totalDraws[i],
        totalGamesPlayed: input.totalWins[i] + input.totalLosses[i] + input.totalDraws[i],
        performance: (input.totalWins[i] - input.totalLosses[i]) / (input.totalWins[i] + input.totalLosses[i] + input.totalDraws[i])
    }});
    output.players.sort((a, b) => {
        if(a.performance > b.performance) return 1;
        if(a.performance < b.performance) return -1;
        return 0;
    });
    console.log(output.players);
    // what now?
    // i need to unpack the matchup winners things
    // i guess i can just replace all the indices with the proper ids (and undefined for draws)
    // and replace the matchup indices with matchup objects
    output.matchups = input.matchupWinners.map((v, i) => {
        return { };
    });
    return input;
}