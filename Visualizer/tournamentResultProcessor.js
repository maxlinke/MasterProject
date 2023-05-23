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
        totalGamesPlayed: playerGameCounts[i],
        winPercentage: input.totalWins[i] / playerGameCounts[i],
        lossPercentage: input.totalLosses[i] / playerGameCounts[i],
        drawPercentage: input.totalDraws[i] / playerGameCounts[i],
        winLossRatio: (input.totalWins[i] - input.totalLosses[i]) / (playerGameCounts[i]),
        elo: input.elo[i]
    }});
    output.matchupSize = input.matchupSize;
    output.matchupRecords = input.matchupRecords;
    output.matchupRecords.forEach(matchupRecord => {
        matchupRecord.playerIds.forEach((id, index) => {
            matchupRecord.playerIds[index] = id.substring(commonPrefix.length);
        });
    });
    return output;
}