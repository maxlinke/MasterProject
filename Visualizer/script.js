'use strict';

document.addEventListener("DOMContentLoaded", () => {

    const matrixWinsOption = "Wins";
    const matrixLossesOption = "Losses";
    const matrixDrawsOption = "Draws";
    const matrixWLBalanceOption = "W/L/D-Balance";
    const displayOptions = [ matrixWinsOption, matrixLossesOption, matrixDrawsOption, matrixWLBalanceOption ];
    const displayOptionDropdown = document.getElementById("displayOptionsSelection");
    function getCurrentDisplayOption () { return displayOptionDropdown.value; }

    const winPercentMetric = "Win%";
    const wlBalanceMetric = "W/L-Balance";
    const eloMetric = "Elo";
    const rankingOptions = [ winPercentMetric, wlBalanceMetric, eloMetric ];
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

    const ignoreMatchupSize = "Ignore";
    const adjustForMatchupSize = "Take Into Account";
    const matchupOptions = [ ignoreMatchupSize, adjustForMatchupSize ];
    const matchupOptionsDropdown = document.getElementById("matchupOptionsSelection");
    function getAdjustForMatchupSize () { return matchupOptionsDropdown.value == adjustForMatchupSize; }

    const rankingColumnLabels = [ "", "Games Played", "Wins", "Wins%", "Losses", "Losses%", "Draws", "Draws%", "W/L-Balance", "Elo" ];
    function getRankingColumnData (playerData, column) {
        switch(column){
            case 0: return playerData.id;
            case 1: return playerData.totalGamesPlayed;
            case 2: return playerData.totalWins;
            case 3: return `${(100 * playerData.winPercentage).toFixed(2)}`;
            case 4: return playerData.totalLosses;
            case 5: return `${(100 * playerData.lossPercentage).toFixed(2)}`;
            case 6: return playerData.totalDraws;
            case 7: return `${(100 * playerData.drawPercentage).toFixed(2)}`;
            case 8: return (getAdjustForMatchupSize() ? playerData.adjustedWinLossBalance : playerData.rawWinLossBalance).toFixed(5);
            case 9: return playerData.elo.toFixed(1);
        }
    }

    let loadedInput = undefined;
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
                onInputLoaded(parsedObject);
            }catch(error){
                // TODO display this somewhere
                console.error(error);
                loadedData = undefined;
            }
        }
        reader.readAsText(evt.target.files[0]);
    }

    function onInputLoaded (input) {
        if(input == undefined){
            loadedInput = undefined;
            loadedData = undefined;
        }else{
            console.log(input);
            loadedInput = input;
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

    function onMatchupOptionChanged () {
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
                case winPercentMetric: aVal = a.winPercentage;     bVal = b.winPercentage;     break;
                case wlBalanceMetric:  aVal = a.rawWinLossBalance; bVal = b.rawWinLossBalance; break;
                case eloMetric:        aVal = a.elo;               bVal = b.elo;               break;
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
                            overlay.className = "matrixDataFieldHighlightOverlay";
                            overlay.style = `border-color: ${info.contrastColor}`;
                        }
                        const newPopup = document.createElement("div");
                        newField.appendChild(newPopup);
                        newPopup.innerHTML = info.popupText;
                        newPopup.className = "matrixFieldPopup";
                        newField.onmouseenter = () => {
                            newPopup.style = "visibility: visible";
                        };
                        newField.onmouseleave = () => {
                            newPopup.style = "";
                        }
                    });
                });
            }
        }
    }

    function getMatrixFieldElementInfoGetterFunction () {
        const errorOutput = {
            color: "#f0f",
            contrastColor: "#fff",
            deterministic: false,
            popupText: "Error"
        };
        const getNoGamesOutput = (mainPlayer, secondaryPlayer, count) => {
            return {
                color: 'transparent',
                contrastColor: 'transparent',
                deterministic: false,
                popupText: getPopupTextForMatrixField(mainPlayer, secondaryPlayer, count)
            }
        }
        switch(getCurrentDisplayOption()){
            case matrixWinsOption:
                return (mainPlayer, secondaryPlayer) => {
                    const records = getMatchupRecordsForMatrixField(mainPlayer, secondaryPlayer);
                    const count = countFirstLetterOccurencesInRecords(records);
                    if(count.total < 1) return getNoGamesOutput(mainPlayer, secondaryPlayer, count);
                    return { 
                        color: `hsl(120, 100%, ${50 * (count["W"][0] / count.total)}%)`, 
                        contrastColor: "#fff",
                        deterministic: count.alwaysZero["W"] || count.alwaysMaximum["W"],
                        popupText: getPopupTextForMatrixField(mainPlayer, secondaryPlayer, count)
                    };
                }
            case matrixLossesOption:
                return (mainPlayer, secondaryPlayer) => {
                    const records = getMatchupRecordsForMatrixField(mainPlayer, secondaryPlayer);
                    const count = countFirstLetterOccurencesInRecords(records);
                    if(count.total < 1) return getNoGamesOutput(mainPlayer, secondaryPlayer, count);
                    return {
                        color: `hsl(0, 100%, ${50 * (count["L"][0] / count.total)}%)`, 
                        contrastColor: "#fff",
                        deterministic: count.alwaysZero["L"] || count.alwaysMaximum["L"],
                        popupText: getPopupTextForMatrixField(mainPlayer, secondaryPlayer, count)
                    };
                }
            case matrixDrawsOption:
                return (mainPlayer, secondaryPlayer) => {
                    const records = getMatchupRecordsForMatrixField(mainPlayer, secondaryPlayer);
                    const count = countFirstLetterOccurencesInRecords(records);
                    if(count.total < 1) return getNoGamesOutput(mainPlayer, secondaryPlayer, count);
                    return {
                        color: `hsl(0, 0%, ${50 * (count["D"][0] / count.total)}%)`,
                        contrastColor: "#fff",
                        deterministic: count.alwaysZero["D"] || count.alwaysMaximum["D"],
                        popupText: getPopupTextForMatrixField(mainPlayer, secondaryPlayer, count)
                    };
                }
            case matrixWLBalanceOption:
                let getWlRatio;
                if(getAdjustForMatchupSize()){
                    getWlRatio = getAdjustedWinLossBalance;
                }else{
                    getWlRatio = (w, l, _) => (w - l) / Math.max(w + l, 1);
                }
                return (mainPlayer, secondaryPlayer) => {
                    const records = getMatchupRecordsForMatrixField(mainPlayer, secondaryPlayer);
                    const count = countFirstLetterOccurencesInRecords(records);
                    if(count.total < 1) return getNoGamesOutput(mainPlayer, secondaryPlayer, count);
                    // const rawWLRatio = (count["W"][0] - count["L"][0]) / Math.max(1, (count.total - count["D"][0]));
                    const wlRatio = getWlRatio(count["W"][0], count["L"][0], loadedData.matchupSize);
                    const hue = 120 * ((wlRatio + 1) / 2);
                    const saturation = 100 * (1 - (count["D"][0] / count.total));
                    return {
                        color: `hsl(${hue}, ${saturation}%, 50%)`,
                        contrastColor: "#fff",   
                        deterministic: count.alwaysMaximum["W"] || count.alwaysMaximum["L"] || count.alwaysMaximum["D"],
                        popupText: getPopupTextForMatrixField(mainPlayer, secondaryPlayer, count)
                    };
                }
            default:
                return () => { return errorOutput };
        }
    }

    const gameResultCharacters = [ "W", "L", "D" ];
    const gameResultCharacterMeanings = [ "Wins", "Losses", "Draws" ];

    function countFirstLetterOccurencesInRecords (records) {
        const output = { total: 0, alwaysZero: [], alwaysMaximum: [] };
        gameResultCharacters.forEach(key => {
            output[key] = Array(loadedData.matchupSize).fill(0);
        });
        records.forEach(record => {
            record.gameResults.forEach(resultString => {
                [...resultString].forEach((character, index) => {
                    output[character][index]++;
                });
            });
            output.total += record.gameResults.length;
        });
        gameResultCharacters.forEach(key => {
            output.alwaysZero[key] = (output[key][0] == 0);
            output.alwaysMaximum[key] = (output[key][0] == output.total);
        });
        return output
    }

    function getPopupTextForMatrixField (mainPlayer, secondaryPlayer, count) {
        const fieldPlayers = [ mainPlayer.id, secondaryPlayer.id ].concat(...additionalMatrixDimensionPlayerIds);
        let output = "";
        fieldPlayers.forEach((player, matchupPlayerIndex) => {
            output += `Player ${matchupPlayerIndex+1}: ${player}\n`;
            if(count.total > 0){
                gameResultCharacters.forEach((character, gameResultCharacterIndex) => {
                    const p = count[character][matchupPlayerIndex] / count.total;
                    if(p == 0 || p == 1){
                        output += `\t${gameResultCharacterMeanings[gameResultCharacterIndex]}: <b>${(100 * p).toFixed(2)}%</b>\n`;
                    }else{
                        output += `\t${gameResultCharacterMeanings[gameResultCharacterIndex]}: ${(100 * p).toFixed(2)}%\n`;
                    }
                });
            }
        });
        if(count.total < 1){
            output += 'No data for matchup';
        }else{
            output += `${count.total} games played in this matchup`;
        }
        return output;
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
    initDropdown(matchupOptionsDropdown, matchupOptions, onMatchupOptionChanged);
    try{
        onInputLoaded(testData);
    }catch(e){
        onInputLoaded(undefined);
        if(!(e instanceof ReferenceError)) throw e;
    }

});