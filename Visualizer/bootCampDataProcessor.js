'use strict';

function processBootCampData (input) {
    const output = {};
    const allAgentIds = [];
    input.generations.forEach(gen => {
        gen.forEach(individual => {
            allAgentIds.push(individual.agentId);        
        });
    });
    const agentIdPrefix = findCommonNamespacePrefix(allAgentIds);
    output.individualTypes = [ "New Random", "Mutation", "Combination", "Clone", "Inverted Clone" ];  // just a copy of the c# enum
    output.individualTypeColors = {};
    output.individualTypeColors[output.individualTypes[0]] = "#af0";    // yellow-green
    output.individualTypeColors[output.individualTypes[1]] = "#f00";    // red
    output.individualTypeColors[output.individualTypes[2]] = "#0af";    // blue
    output.individualTypeColors[output.individualTypes[3]] = "#fa0";    // orange
    output.individualTypeColors[output.individualTypes[4]] = "#a0f";    // purple
    output.generations = input.generations.map((generation, generationIndex) => {
        return generation.map((individual, individualIndex) => {
            const unknownProps = getUnknownProperties(individual);
            const outputIndividual = {};    // we're only copying what we need and format the rest to a nice string
            outputIndividual.guid = individual.guid;
            outputIndividual.agentId = individual.agentId.substring(agentIdPrefix.length);
            outputIndividual.individualType = output.individualTypes[individual.IndividualType];
            outputIndividual.fitness = individual.finalFitness;
            outputIndividual.parentGuids = [...individual.parentGuids];
            outputIndividual.lineageGuids = [...individual.parentGuids];
            appendRemainingLineageGuids(outputIndividual, generationIndex);
            outputIndividual.percentages = {};
            // make these as 
            // percentages["Wins%"]["Total"]
            // pergentages["Wins%"]["Peers"];
            // that makes the visualizer easier
            unknownProps.forEach(unknownProp => { outputIndividual[unknownProp] = individual[unknownProp]; });  // but we do copy all unknown data so people can "make their own visualizations" for numbers
            outputIndividual.popupText = getPopupText(outputIndividual, ["guid", "parentGuids", "lineageGuids"]);
            if(generationIndex == 0 && individualIndex == 0){   // just for tests
                console.log(outputIndividual.popupText);
            }
            return outputIndividual;
        });
    });
    return output;

    function appendRemainingLineageGuids (individual, individualGenerationIndex) {
        let lineageGenIndex = individualGenerationIndex - 1;
        while(lineageGenIndex >= 0){
            input.generations[lineageGenIndex].forEach((prevGenIndividual) => {
                if(individual.lineageGuids.includes(prevGenIndividual.guid)){
                    prevGenIndividual.parentGuids.forEach((lineageGuid) => {
                        if(!individual.lineageGuids.includes(lineageGuid)){
                            individual.lineageGuids.push(lineageGuid);
                        }
                    });
                }
            });
            lineageGenIndex--;
        };
    }

    function getUnknownProperties (individual) {
        const output = [];
        for(const propertyName in individual){
            switch(propertyName){
                case "IndividualType":
                case "agentId":
                case "finalFitness":
                case "guid":
                case "parentGuids":
                case "peerTournamentResult":
                case "randomTournamentResult":
                    break;
                default:
                    output.push(propertyName);
                    break;
            }
        }
        return output;
    }

    function getPopupText (individual, propsToIgnore) {
        const lines = [];
        for(const propertyName in individual){
            if(!propsToIgnore.includes(propertyName)){
                const nicePropName = getNicePropertyName(propertyName);
                if(typeof(individual[propertyName]) == "object"){
                    lines.push(`${nicePropName}:`);
                    appendSubProps(individual[propertyName], 1);
                }else{
                    lines.push(`${nicePropName}: ${individual[propertyName]}`);
                }
            }
        }
        return lines.join("\n");

        function appendSubProps (subProp, depth) {
            const indent = " ".repeat(depth * 3);
            for(const deeperProp in subProp){
                const nicePropName = getNicePropertyName(deeperProp);
                if(typeof(deeperProp) == "object"){
                    lines.push(`${indent}${nicePropName}:`);
                    appendSubProps(deeperProp, depth+1);
                }else{
                    lines.push(`${indent}${nicePropName}: ${subProp[deeperProp]}`);
                }
            }
        }

        function getNicePropertyName (rawPropertyName) {
            const output = [];
            if(rawPropertyName.length > 0){
                rawPropertyName = rawPropertyName.charAt(0).toUpperCase() + rawPropertyName.substring(1);
                let substringStart = 0;
                let previousWasUppercase = true;
                for(let i=1; i<rawPropertyName.length; i++){
                    const character = rawPropertyName.charAt(i);
                    const isUppercase = (character == character.toUpperCase());
                    if(isUppercase && !previousWasUppercase){
                        output.push(rawPropertyName.substring(substringStart, i));
                        substringStart = i;
                    }
                    previousWasUppercase = isUppercase;
                }
                output.push(rawPropertyName.substring(substringStart, rawPropertyName.length));
            }
            return output.join(" ");
        }
    }
}