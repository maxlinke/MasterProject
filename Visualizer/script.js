'use strict';

document.addEventListener("DOMContentLoaded", () => {

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
            }
        }
        reader.readAsText(evt.target.files[0]);
    }

    function onDataLoaded (input) {
        console.log(input);
    }

    document.getElementById("fileInput").addEventListener("change", handleFileSelect, false);

});