'use strict';

document.addEventListener("DOMContentLoaded", () => {

    const winsId = "Wins";
    const lossesId = "Losses";
    const drawsId = "Draws";

    const displayOptions = [ winsId, lossesId, drawsId ];
    const displayOptionDropdown = document.getElementById("displayOptionsSelection");
    function getCurrentDisplayOption () { return displayOptionDropdown.value; }

    const wlRatioMetric = "(Wins-Losses)/Games";
    const eloMetric = "Elo (TODO)";
    const performanceMetrics = [ wlRatioMetric, eloMetric ];
    // TODO dropdown for that too
    // so don't sort the participant ids in the processing step
    // but rather just assign the scores there
    // and the sorting can happen in here
    // show the raw data in a different table (i'll have to refactor my code in here)
    // agentid wins losses draws w/l elo

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
                clearTable();
            }
        }
        reader.readAsText(evt.target.files[0]);
    }

    let loadedData = undefined;

    function onDataLoaded (input) {
        clearTable();
        loadedData = processData(input);
        console.log(loadedData);
        updateTable();
        // TODO add the option to select participants if more than 2 players per matchup (include "any" option to average all together)
    }

    function onDisplayOptionChanged () {
        console.log(getCurrentDisplayOption());
        if(loadedData != undefined){
            updateTable();
        }
    }

    function clearTable () {
        const tables = document.getElementsByTagName("table");
        for(let i=0; i<tables.length; i++){
            const table = tables[i];
            table.parentElement.removeChild(table);
        }
    }

    function getSimpleWinLossDrawRatio (wins, losses, draws) {
        const output = [];
        for(let i=0; i<wins.length; i++){
            output[i] = (wins[i] - losses[i]) / (wins[i] + losses[i] + draws[i]);
        }
        return output;
    }

    function updateTable () {
        clearTable();
        // TODO data will be processed in above step
        // document.getElementsByTagName("body")[0].appendChild(...
    }

//  ----- init -----

    document.getElementById("fileInput").addEventListener("change", handleFileSelect, false);
    displayOptions.forEach((option) => {
        const newOption = document.createElement("option");
        displayOptionDropdown.appendChild(newOption);
        newOption.value = option;
        newOption.innerHTML = option;
    });
    displayOptionDropdown.value = displayOptions[0];
    displayOptionDropdown.addEventListener("change", () => onDisplayOptionChanged());
    // clearTable();

});