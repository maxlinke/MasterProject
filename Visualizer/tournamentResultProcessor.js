'use strict';

function processTournamentData (input) {
    const output = {};
    const playerGameCounts = input.playerIds.map((id, i) => {
        return input.totalWins[i] + input.totalLosses[i] + input.totalDraws[i];
    });
    const commonPrefix = findCommonNamespacePrefix(input.playerIds);
    output.players = input.playerIds.map((id, i) => { return { 
        id: id.substring(commonPrefix.length),
        index: i,
        totalGamesPlayed: playerGameCounts[i],
        totalWins: input.totalWins[i],
        totalLosses: input.totalLosses[i],
        totalDraws: input.totalDraws[i],
        winPercentage: input.totalWins[i] / playerGameCounts[i],
        lossPercentage: input.totalLosses[i] / playerGameCounts[i],
        drawPercentage: input.totalDraws[i] / playerGameCounts[i],
        rawWinLossBalance: (input.totalWins[i] - input.totalLosses[i]) / Math.max(1, (playerGameCounts[i] - input.totalDraws[i])),
        adjustedWinLossBalance: getAdjustedWinLossBalance(input.totalWins[i], input.totalLosses[i], Math.max(2, input.matchupSize)), 
        elo: input.elo[i]
    }});
    output.matchupSize = input.matchupSize;
    output.matchupRecords = JSON.parse(JSON.stringify(input.matchupRecords));   // create a clone, because otherwise reloading loaded data becomes problematic
    output.matchupRecords.forEach(matchupRecord => {
        matchupRecord.playerIds.forEach((id, index) => {
            matchupRecord.playerIds[index] = id.substring(commonPrefix.length);
        });
    });
    return output;
}

function getAdjustedWinLossBalance (wins, losses, matchupSize) {
    let nonDrawTotal = wins + losses;
    let expectedWins = (1 / matchupSize) * nonDrawTotal;
    if(wins >= expectedWins){
        return (wins - expectedWins) / Math.max(1, (nonDrawTotal - expectedWins));    // from 0 to 1
    }else{
        let expectedLosses = ((matchupSize - 1) / matchupSize) * nonDrawTotal;
        return -1 * (losses - expectedLosses) / Math.max(1, (nonDrawTotal - expectedLosses)); // also from 0 to 1, but multiplied with -1
    }
}