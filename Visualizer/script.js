'use strict';

document.addEventListener("DOMContentLoaded", () => {

    const matrixWinsOption = "Wins";
    const matrixLossesOption = "Losses";
    const matrixDrawsOption = "Draws";
    const matrixWLRatioOption = "W/L/D-Ratio";
    const displayOptions = [ matrixWinsOption, matrixLossesOption, matrixDrawsOption, matrixWLRatioOption ];
    const displayOptionDropdown = document.getElementById("displayOptionsSelection");
    function getCurrentDisplayOption () { return displayOptionDropdown.value; }

    const winPercentMetric = "Win%";
    const wlRatioMetric = "W/L-Ratio";
    const eloMetric = "Elo";
    const rankingOptions = [ winPercentMetric, wlRatioMetric, eloMetric ];
    const rankingOptionsDropdown = document.getElementById("rankingOptionsSelection");
    function getCurrentRankingOption () { return rankingOptionsDropdown.value; }

    const noHighlights = "Off";
    const fixedOutcomeHighlights = "Fixed Outcomes";
    const highlightOptions = [ noHighlights, fixedOutcomeHighlights ];
    const highlightOptionsDropdown = document.getElementById("highlightOptionsSelection");
    function getHighlightFixedOutcomes () { return highlightOptionsDropdown.value == fixedOutcomeHighlights; }

    const rankingTable = document.getElementById("agentRankingTable");
    const matchupMatrix = document.getElementById("matchupMatrix");
    const matchupMatrixControlsParent = document.getElementById("additionalMatchupMatrixControls");

    const rankingColumnLabels = [ "", "Wins", "Wins%", "Losses", "Losses%", "Draws", "Draws%", "W/L-Ratio", "Elo" ];
    function getRankingColumnData (playerData, column) {
        switch(column){
            case 0: return playerData.id;
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
    let additionalMatrixDimensionPlayerIds = [];

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
        if(input == undefined){
            loadedData = undefined;
        }else{
            console.log(input);
            loadedData = processData(input);
            console.log(loadedData);
        }
        updateRankingTable();
        updateAdditionalMatrixControls();
        updateMatrix();
    }

    function onMatrixOptionChanged () {
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

    const allPlayersId = "Any"

    function updateAdditionalMatrixControls () {
        matchupMatrixControlsParent.replaceChildren();
        additionalMatrixDimensionPlayerIds = [];
        const matrixLimit = 2;  // can be lowered to display the dropdowns for testing
        if(loadedData != undefined && loadedData.matchupSize > matrixLimit){
            let additionalAxisOptions = [ allPlayersId ].concat(loadedData.players.map((v, i) => { return v.id; }));
            for(let i=0; i<loadedData.matchupSize-matrixLimit; i++){
                const dropdownId = `additionalMatrixAxis${i}`;
                const newDropdownParent = document.createElement("div");
                newDropdownParent.className = "horizontalControlGroup";
                matchupMatrixControlsParent.appendChild(newDropdownParent);
                const newDropdownLabel = document.createElement("label");
                newDropdownLabel.htmlFor = dropdownId;
                newDropdownLabel.innerHTML = `Axis ${i+matrixLimit}`;
                newDropdownParent.appendChild(newDropdownLabel);
                newDropdownParent.appendChild(document.createTextNode("\n"));
                const newDropdown = document.createElement("select");
                newDropdown.id = dropdownId;
                newDropdownParent.appendChild(newDropdown);
                initDropdown(newDropdown, additionalAxisOptions, () => {
                    additionalMatrixDimensionPlayerIds[i] = newDropdown.value;
                    updateMatrix();
                });
                additionalMatrixDimensionPlayerIds.push(newDropdown.value);
            }
        }
    }

    function updateRankingTable () {
        rankingTable.replaceChildren();
        if(loadedData != undefined && loadedData.players.length > 0){
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
    }

    function updateMatrix () {
        matchupMatrix.replaceChildren();
        if(loadedData != undefined){
            if(loadedData.matchupSize >= 2){
                const getInfo = getMatrixFieldElementInfoGetterFunction();
                const headerRow = document.createElement("tr");
                matchupMatrix.appendChild(headerRow);
                headerRow.appendChild(document.createElement("th"));    // empty field
                sortedPlayerIndices.forEach((playerIndex) => {
                    const newLabel = document.createElement("th");
                    headerRow.appendChild(newLabel);
                    newLabel.innerHTML = loadedData.players[playerIndex].id[0];
                    newLabel.className = "matrixColumnLabel";
                });
                const highlightFixed = getHighlightFixedOutcomes();
                sortedPlayerIndices.forEach((mainPlayerIndex) => {
                    const mainPlayer = loadedData.players[mainPlayerIndex];
                    const newRow = document.createElement("tr");
                    matchupMatrix.appendChild(newRow);
                    const newLabel = document.createElement("th");
                    newRow.appendChild(newLabel);
                    newLabel.innerHTML = mainPlayer.id;
                    newLabel.className = "matrixRowLabel";
                    sortedPlayerIndices.forEach((secondaryPlayerIndex) => {
                        const secondaryPlayer = loadedData.players[secondaryPlayerIndex];
                        const newField = document.createElement("td");
                        newRow.appendChild(newField);
                        newField.className = "matrixDataField";
                        const info = getInfo(mainPlayer, secondaryPlayer);
                        newField.style = `background-color: ${info.color}`;
                        if(info.deterministic && highlightFixed){
                            const overlay = document.createElement("div");
                            newField.appendChild(overlay);
                            overlay.style = `position: relative; padding: 0; margin-bottom: 0; top: 0; left: 0; width: 100%; height: 100%; box-sizing: border-box; border: 2px solid ${info.contrastColor}; border-collapse: collapse; border-radius: 8px;`;
                        }
                    });
                });
            }
        }
    }

    function getMatrixFieldElementInfoGetterFunction () {
        const errorCol = "#FF00FF"
        switch(getCurrentDisplayOption()){
            case matrixWinsOption:
                return (mainPlayer, secondaryPlayer) => {
                    const records = getMatchupRecordsForMatrixField(mainPlayer, secondaryPlayer);
                    const count = countFirstLetterOccurencesInRecords(records);
                    if(count.games < 1) return errorCol;
                    return { 
                        color: `hsl(120, 100%, ${50 * (count["W"] / count.total)}%)`, 
                        contrastColor: "#fff",
                        deterministic: count.alwaysZero["W"] || count.alwaysMaximum["W"]
                    };
                }
            case matrixLossesOption:
                return (mainPlayer, secondaryPlayer) => {
                    const records = getMatchupRecordsForMatrixField(mainPlayer, secondaryPlayer);
                    const count = countFirstLetterOccurencesInRecords(records);
                    if(count.games < 1) return errorCol;
                    return {
                        color: `hsl(0, 100%, ${50 * (count["L"] / count.total)}%)`, 
                        contrastColor: "#fff",
                        deterministic: count.alwaysZero["L"] || count.alwaysMaximum["L"]
                    };
                }
            case matrixDrawsOption:
                return (mainPlayer, secondaryPlayer) => {
                    const records = getMatchupRecordsForMatrixField(mainPlayer, secondaryPlayer);
                    const count = countFirstLetterOccurencesInRecords(records);
                    if(count.games < 1) return errorCol;
                    return {
                        color: `hsl(0, 0%, ${50 * (count["D"] / count.total)}%)`,
                        contrastColor: "#fff",
                        deterministic: count.alwaysZero["D"] || count.alwaysMaximum["D"]
                    };
                }
            case matrixWLRatioOption:
                return (mainPlayer, secondaryPlayer) => {
                    const records = getMatchupRecordsForMatrixField(mainPlayer, secondaryPlayer);
                    const count = countFirstLetterOccurencesInRecords(records);
                    if(count.games < 1) return errorCol;
                    const rawWLRatio = (count["W"] - count["L"]) / Math.max(1, (count.total - count["D"]));
                    const hue = 120 * ((rawWLRatio + 1) / 2);
                    const saturation = 100 * (1 - (count["D"] / count.total));
                    return {
                        color: `hsl(${hue}, ${saturation}%, 50%)`,
                        contrastColor: "#fff",   
                        deterministic: count.alwaysMaximum["W"] || count.alwaysMaximum["L"] || count.alwaysMaximum["D"]
                    };
                }
            default:
                return () => { return { 
                        color: errorCol, 
                        contrastColor: "#fff",
                        deterministic: false 
                    };
                };
        }
    }

    function countFirstLetterOccurencesInRecords (records) {
        const output = { total: 0, alwaysZero: [], alwaysMaximum: [] };
        records.forEach(record => {
            record.gameResults.forEach(resultString => {
                const key = resultString[0];
                if(!output.hasOwnProperty(key)){
                    output[key] = 0;
                }
                output[key]++;
            });
            output.total += record.gameResults.length;
        });
        [ "W", "L", "D" ].forEach(key => {
            if(output[key] == undefined) output[key] = 0;
            output.alwaysZero[key] = (output[key] == 0);
            output.alwaysMaximum[key] = (output[key] == output.total);
        });
        return output
    }

    function getMatchupRecordsForMatrixField (mainPlayer, secondaryPlayer) {
        let basePair = [ mainPlayer.id, secondaryPlayer.id ];
        let matchups = [ basePair ];
        additionalMatrixDimensionPlayerIds.forEach((additionalPlayerId, index) => {
            if(additionalPlayerId == allPlayersId){
                let newOutput = [];
                matchups.forEach(group => {
                    loadedData.players.forEach((additionalPlayer) => {
                        newOutput.push(group.concat(additionalPlayer.id));
                    });
                });
                matchups = newOutput;
            }else{
                matchups.forEach(group => {
                    group.push(additionalPlayerId);
                });
            }
        });
        let output = [];
        matchups.forEach((matchup) => {
            loadedData.matchupRecords.forEach((matchupRecord) => {
                let isCorrectMatchup = true;
                for(let i=0; i<matchup.length; i++){
                    if(matchup[i] != matchupRecord.playerIds[i]){
                        isCorrectMatchup = false;
                        break;
                    }
                }
                if(isCorrectMatchup){
                    output.push(matchupRecord);
                }
            });
        });
        return output;
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
    initDropdown(rankingOptionsDropdown, rankingOptions, onRankingOptionChanged);
    initDropdown(displayOptionDropdown, displayOptions, onMatrixOptionChanged);
    initDropdown(highlightOptionsDropdown, highlightOptions, onMatrixOptionChanged);
    try{
        onDataLoaded(testData);
    }catch(e){
        onDataLoaded(undefined);
        if(!(e instanceof ReferenceError)) throw e;
    }

});