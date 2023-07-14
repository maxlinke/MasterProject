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
    output.unknownVisualizableProperties = {};
    output.generations = input.generations.map((generation, genIndex) => {
        return generation.map((individual, individualIndex) => {
            const outputIndividual = {};    // we're only copying what we need and format the rest to a nice string
            outputIndividual.guid = individual.guid;
            outputIndividual.agentId = replaceCaretCharacters(individual.agentId.substring(agentIdPrefix.length));
            outputIndividual.individualType = output.individualTypes[individual.IndividualType];
            outputIndividual.generation = genIndex;
            outputIndividual.fitness = individual.finalFitness;
            outputIndividual.parentGuids = [...individual.parentGuids];
            outputIndividual.lineageGuids = [...individual.parentGuids];
            outputIndividual.tournamentResults = {};
            outputIndividual.tournamentResults["All"] = processTournamentResult(combineRawTournamentResults(individual.peerTournamentResult, individual.randomTournamentResult));
            outputIndividual.tournamentResults["Peers"] = processTournamentResult(individual.peerTournamentResult);
            outputIndividual.tournamentResults["Random"] = processTournamentResult(individual.randomTournamentResult);
            const unknownProps = getUnknownProperties(individual);
            unknownProps.forEach(unknownProp => {
                outputIndividual[unknownProp] = individual[unknownProp];    // copy these over so they appear in the popup text
                ensureVisualizablePropertiesRegistered(outputIndividual[unknownProp], [ unknownProp ]);
            });
            outputIndividual.popupText = getPopupText(outputIndividual, ["parentGuids", "lineageGuids"]);
            return outputIndividual;
        });
    });
    output.generations.forEach((generation, generationIndex) => {   // lineage must be done after the first iteration because it accesses the other individuals which aren't there yet in the initial map-call above
        generation.forEach(individual => {                          // the fact that this happens after the popup text is generated does not matter, because lineage is excluded from that
            appendRemainingLineageGuids(individual, generationIndex);
        });
    });
    return output;

    function appendRemainingLineageGuids (individual, individualGenerationIndex) {
        let lineageGenIndex = individualGenerationIndex - 1;
        while(lineageGenIndex >= 0){
            output.generations[lineageGenIndex].forEach((prevGenIndividual) => {
                if(individual.lineageGuids.includes(prevGenIndividual.guid)){
                    prevGenIndividual.parentGuids.forEach((lineageGuid) => {
                        if(!individual.lineageGuids.includes(lineageGuid)){
                            individual.lineageGuids.push(lineageGuid);
                        }
                    });
                    if(!prevGenIndividual.lineageGuids.includes(individual.guid)){
                        prevGenIndividual.lineageGuids.push(individual.guid);
                    }
                }
            });
            lineageGenIndex--;
        };
    }

    function combineRawTournamentResults (a, b) {
        return {
            totalWins: a.totalWins + b.totalWins,
            totalDraws: a.totalDraws + b.totalDraws,
            totalLosses: a.totalLosses + b.totalLosses
        };
    }

    function processTournamentResult (tournamentResult) {
        const tw = tournamentResult.totalWins;
        const td = tournamentResult.totalDraws;
        const tl = tournamentResult.totalLosses;
        const total = tw + td + tl;
        return {
            Wins: getSubResult(tw),
            Draws: getSubResult(td),
            Losses: getSubResult(tl),
            Total: total,
            popupText: `${tw} wins, ${td} draws, ${tl} losses`
        };

        function getSubResult (invidiualCount) {
            return {
                count: invidiualCount,
                percentage: (100 * invidiualCount) / Math.max(total)
            }
        }
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

    function ensureVisualizablePropertiesRegistered (prop, keyStack) {
        const keyAsString = keyStack.join(".");
        if(output.unknownVisualizableProperties[keyAsString] == undefined){
            if(isFinite(prop)){
                output.unknownVisualizableProperties[keyAsString] = {
                    dropdownText : keyAsString,
                    getValueFromIndividual: (individual) => {
                        let prop = individual;
                        keyStack.forEach(key => {
                            prop = prop[key];
                        });
                        return Number(prop);
                    }
                };
            }else{
                if(typeof(prop) == "object"){
                    for(const subKey in prop){
                        ensureVisualizablePropertiesRegistered(prop[subKey], keyStack.concat([subKey]));
                    }
                }
            }
        }
    }

    function getPopupText (individual, propsToIgnore) {
        const lines = [];
        for(const propertyName in individual){
            if(!propsToIgnore.includes(propertyName)){
                const nicePropName = getNicePropertyName(propertyName);
                if(propertyName == "tournamentResults"){
                    lines.push(`${nicePropName}:`);
                    appendSubPropsText(individual[propertyName], 1, (result) => { return result.popupText; });
                }else{
                    if(typeof(individual[propertyName]) == "object"){
                        lines.push(`${nicePropName}:`);
                        appendSubPropsText(individual[propertyName], 1);
                    }else{
                        lines.push(`${nicePropName}: ${individual[propertyName]}`);
                    }
                }
            }
        }
        return lines.join("\n");

        function appendSubPropsText (subProp, depth, subPropToString) {
            const indent = " ".repeat(depth * 3);
            for(const deeperProp in subProp){
                const nicePropName = getNicePropertyName(deeperProp);
                if(subPropToString == undefined){
                    if(typeof(subProp[deeperProp]) == "object"){
                        lines.push(`${indent}${nicePropName}:`);
                        appendSubPropsText(subProp[deeperProp], depth+1);
                    }else{
                        lines.push(`${indent}${nicePropName}: ${subProp[deeperProp]}`);
                    }
                }else{
                    lines.push(`${indent}${nicePropName}: ${subPropToString(subProp[deeperProp])}`);
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