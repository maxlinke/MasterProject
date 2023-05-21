'use strict';

document.addEventListener("DOMContentLoaded", () => {

    const winsId = "Wins";
    const lossesId = "Losses";
    const drawsId = "Draws";

    const displayOptions = [ winsId, lossesId, drawsId ];
    const displayOptionDropdown = document.getElementById("displayOptionsSelection");
    function getCurrentDisplayOption () { return displayOptionDropdown.value; }

    async function handleFileSelect(evt) {
        const reader = new FileReader();
        reader.onload = function(e) {
            try{
                const rawText = e.target.result;
                const parsedObject = JSON.parse(rawText);
                onDataLoaded(parsedObject);
            }catch(error){
                // TODO display this somewhere
                console.error(error);
                clearTable();
            }
        }
        reader.readAsText(evt.target.files[0]);
    }

    let loadedData = undefined;

    function onDataLoaded (input) {
        clearTable();
        if(!validateData(input)){
            loadedData = undefined;
            throw "lol";
        }
        loadedData = processData(input);;
        updateTable();
        // TODO add the option to select participants if more than 2 players per matchup (include "any" option to average all together)
    }

    function validateData (input) {
        // TODO check that everything is okay at first glance
        // on the other hand, if i can limit the file select to specific extensions i can just make them .tournamentResult
        // keep the json inside but change the extension to make the dropdown better
        // then this validation won't be neccessary
        return true;
    }

    function processData (input) {
        // TODO unpack the compact data
        return input;
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

    function getParticipantsSortedByMetric (participants, values) {
        const map = participants.map((p, i) => {
            return { participantName: p, value: values[i] }
        });
        map.sort((a, b) => {
            if(a.value > b.value) return 1;
            if(b.value < b.value) return -1;
            return 0;
        });
        return map.map((obj) => obj.participantName);
    }

    function updateTable () {
        clearTable();
        console.log(loadedData);
        const sortedParticipants = getParticipantsSortedByMetric(loadedData.playerIds, loadedData.totalWins);
        console.log(sortedParticipants);
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
    clearTable();

});