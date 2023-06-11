'use strict';

function processData (input) {
    // TODO unpack the compact data
    const output = {};
    const playerGameCounts = input.playerIds.map((id, i) => {
        return input.totalWins[i] + input.totalLosses[i] + input.totalDraws[i];
    });
    let shortestIdLength = Infinity;
    let shortestId = "";
    input.playerIds.forEach(id => {
        if(id.length < shortestIdLength){
            shortestIdLength = id.length;
            shortestId = id;
        }
    });
    let commonPrefix = "";
    if(shortestId.includes(".")){
        commonPrefix = shortestId.substring(0, shortestId.lastIndexOf(".") + 1);
        input.playerIds.forEach(id => {
            while(commonPrefix.length > 0){
                if(id.startsWith(commonPrefix)){
                    return;
                }
                commonPrefix = commonPrefix.substring(0, commonPrefix.length - 1);
                commonPrefix = commonPrefix.substring(0, commonPrefix.lastIndexOf(".") + 1);
            }
        });
    }
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