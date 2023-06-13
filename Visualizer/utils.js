'use strict';

function findCommonNamespacePrefix (input) {
    let shortestLength = Infinity;
    let shortestValue = "";
    input.forEach(value => {
        if(value.length < shortestLength){
            shortestLength = value.length;
            shortestValue = value;
        }
    });
    let commonPrefix = "";
    if(shortestValue.includes(".")){
        commonPrefix = shortestValue.substring(0, shortestValue.lastIndexOf(".") + 1);
        input.forEach(value => {
            while(commonPrefix.length > 0){
                if(value.startsWith(commonPrefix)){
                    return;
                }
                commonPrefix = commonPrefix.substring(0, commonPrefix.length - 1);
                commonPrefix = commonPrefix.substring(0, commonPrefix.lastIndexOf(".") + 1);
            }
        });
    }
    return commonPrefix;
}

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