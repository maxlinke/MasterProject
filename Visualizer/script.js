'use strict';

document.addEventListener("DOMContentLoaded", () => {

    const fileInput = document.getElementById("fileInput");
    function getCurrentFileName () { 
        const fullPath = fileInput.value; 
        const firstSplit = fullPath.split("/");
        const secondSplit = firstSplit[firstSplit.length - 1].split("\\");
        return secondSplit[secondSplit.length - 1];
    }

    const fileStatusText = document.getElementById("fileStatus");

    async function handleFileSelect(evt) {
        const reader = new FileReader();
        reader.onload = function(e) {
            try{
                const fileName = getCurrentFileName();
                const extension = fileName.substring(fileName.lastIndexOf('.') + 1);
                const rawText = e.target.result;
                const parsedObject = JSON.parse(rawText);
                onInputLoaded(parsedObject, extension);
                fileStatusText.innerHTML = "Loaded";
            }catch(error){
                alert("Error, check the console for details!");
                console.error(error);
                fileStatusText.innerHTML = "Error";
            }
        }
        fileStatusText.innerHTML = "Loading";
        reader.readAsText(evt.target.files[0]);
    }

    function onInputLoaded (input, extension) {
        resetPage();
        if(input != undefined){
            console.log(input);
            switch(extension){
                case "tournamentResult":
                    appendTemplate("tournamentResultVisualization");
                    onTournamentResultFileLoaded(input);
                    break;
                case "bootCampData":
                    appendTemplate("bootCampDataVisualization");
                    onBootCampDataFileLoaded(input);
                    break;
                default:
                    throw new Error(`Unknown file extension \"${extension}\"!`);
            }
        }
    }

    function resetPage () {
        const bodyChildren = document.body.children;
        const childrenToRemove = [];
        for(let i=0; i<bodyChildren.length; i++){
            if(bodyChildren[i].id != "fileSelectionSection"){   // this one is the only one that stays
                childrenToRemove.push(bodyChildren[i]);
            }
        }
        childrenToRemove.forEach(child => child.remove());
    }

    function appendTemplate (templateId) {
        const targetTemplate = document.getElementById(templateId);
        document.body.appendChild(targetTemplate.content.cloneNode(true));
    }

    fileInput.addEventListener("change", handleFileSelect, false);
    try{
        if((typeof testData !== 'undefined') && (typeof testDataType !== 'undefined')){
            onInputLoaded(testData, testDataType);
        }else{
            onInputLoaded(undefined);
        }
    }catch(e){
        alert("Error!");
        onInputLoaded(undefined);
        throw e;
    }

});