'use strict';

document.addEventListener("DOMContentLoaded", () => {

    const winsId = "Wins";
    const lossesId = "Losses";
    const drawsId = "Draws";
    const displayOptions = [ winsId, lossesId, drawsId ];
    const displayOptionDropdown = document.getElementById("displayOptionsSelection");
    function getCurrentDisplayOption () { return displayOptionDropdown.value; }

    const winPercentMetric = "Win%";
    const wlRatioMetric = "W/L-Ratio";
    const eloMetric = "Elo";
    const rankingOptions = [ winPercentMetric, wlRatioMetric, eloMetric ];
    const rankingOptionsDropdown = document.getElementById("rankingOptionsSelection");
    function getCurrentRankingOption () { return rankingOptionsDropdown.value; }

    const rankingTable = document.getElementById("agentRankingTable");
    const matchupMatrix = document.getElementById("matchupMatrix");

    const rankingColumnLabels = [ "", "Wins", "Wins%", "Losses", "Losses%", "Draws", "Draws%", "W/L-Ratio", "Elo" ];
    function getRankingColumnData (playerData, column) {
        switch(column){
            case 0: return playerData.shortId;
            case 1: return playerData.totalWins;
            case 2: return `${(100 * playerData.winPercentage).toFixed(2)}`;
            case 3: return playerData.totalLosses;
            case 4: return `${(100 * playerData.lossPercentage).toFixed(2)}`;
            case 5: return playerData.totalDraws;
            case 6: return `${(100 * playerData.drawPercentage).toFixed(2)}`;
            case 7: return playerData.winLossRatio.toFixed(5);
            case 8: return playerData.elo.toFixed(1);
        }
    }

    let loadedData = undefined;
    let sortedPlayerIndices = [];

    async function handleFileSelect(evt) {
        const reader = new FileReader();
        reader.onload = function(e) {
            try{
                // TODO if i support also displaying not just tournament results but other things (agent records?) i need to discriminate what file i just opened
                const rawText = e.target.result;
                const parsedObject = JSON.parse(rawText);
                onDataLoaded(parsedObject);
            }catch(error){
                // TODO display this somewhere
                console.error(error);
                loadedData = undefined;
            }
        }
        reader.readAsText(evt.target.files[0]);
    }

    function onDataLoaded (input) {
        loadedData = processData(input);
        console.log(loadedData);
        updateRankingTable();
        updateMatrix();
        // TODO add the option to select participants if more than 2 players per matchup (include "any" option to average all together)
    }

    function onDisplayOptionChanged () {
        if(loadedData != undefined){
            updateMatrix();
        }
    }

    function onRankingOptionChanged () {
        if(loadedData != undefined){
            updateRankingTable();
            updateMatrix();
        }
    }

    function updateSortedPlayerIndices () {
        const tempMap = loadedData.players.map((v, i) => { return v; });
        const currentRankingOption = getCurrentRankingOption();
        console.log
        tempMap.sort((a, b) => {
            let aVal = NaN;
            let bVal = NaN;
            switch(currentRankingOption){
                case winPercentMetric: aVal = a.winPercentage; bVal = b.winPercentage; break;
                case wlRatioMetric:    aVal = a.winLossRatio;  bVal = b.winLossRatio;  break;
                case eloMetric:        aVal = a.elo;           bVal = b.elo;           break;
            }
            return (aVal > bVal ? 1 : (aVal < bVal) ? -1 : 0);
        });
        sortedPlayerIndices = tempMap.map((v, i) => v.index);
    }

    function updateRankingTable () {
        rankingTable.replaceChildren();
        if(loadedData.players.length > 0){
            updateSortedPlayerIndices();
            const firstRow = document.createElement("tr");
            rankingColumnLabels.forEach(colLabel => {
                const newLabel = document.createElement("th");
                newLabel.className = "rankingTableColumnLabel";
                newLabel.innerHTML = colLabel;
                firstRow.appendChild(newLabel);
            });
            rankingTable.appendChild(firstRow);
            sortedPlayerIndices.forEach(playerIndex => {
                const playerData = loadedData.players[playerIndex];
                const newRow = document.createElement("tr");
                for(let i=0; i<rankingColumnLabels.length; i++){
                    let newData;
                    if(i == 0){
                        newData = document.createElement("th");
                        newData.className = "rankingTableRowLabel";
                    }else{
                        newData = document.createElement("td");
                        newData.className = "rankingTableDataField";
                    }
                    newData.innerHTML = getRankingColumnData(playerData, i);
                    newRow.appendChild(newData);
                }
                rankingTable.appendChild(newRow);
            });
        }
        // TODO
    }

    function updateMatrix () {
        matchupMatrix.replaceChildren();
        // TODO
    }

//  ----- init -----

    function initDropdown (dropdown, options, onValueChanged) {
        options.forEach((option) => {
            const newOption = document.createElement("option");
            dropdown.appendChild(newOption);
            newOption.value = option;
            newOption.innerHTML = option;
        });
        dropdown.value = options[0];
        dropdown.addEventListener("change", () => onValueChanged());
    }

    document.getElementById("fileInput").addEventListener("change", handleFileSelect, false);
    initDropdown(displayOptionDropdown, displayOptions, onDisplayOptionChanged);
    initDropdown(rankingOptionsDropdown, rankingOptions, onRankingOptionChanged);
    // clearTable();
    try{
        onDataLoaded(testData);
    }catch(e){
        if(e instanceof ReferenceError) console.log("no test data");
        else throw e;
    }

});