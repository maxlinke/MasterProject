'use strict';

function processBootCampData (input) {
    const output = {};
    const allAgentIds = [];
    input.generations.map(gen => {
        gen.forEach(individual => {
            allAgentIds.push(individual.agentId);        
        });
    });
    const agentIdPrefix = findCommonNamespacePrefix(allAgentIds);
    output.individualTypes = [ "New Random", "Mutation", "Combination", "Clone", "InvertedClone" ];  // just a copy of the c# enum
    output.individualTypeColors = {};
    output.individualTypeColors[output.individualTypes[0]] = "#af0";    // yellow-green
    output.individualTypeColors[output.individualTypes[1]] = "#f00";    // red
    output.individualTypeColors[output.individualTypes[2]] = "#0af";    // blue
    output.individualTypeColors[output.individualTypes[3]] = "#fa0";    // orange
    output.individualTypeColors[output.individualTypes[4]] = "#a0f";    // purple
    output.generations = input.generations.map((generation, generationIndex) => {
        return generation.map((individual, individualIndex) => {
            individual.individualType = output.individualTypes[individual.IndividualType];
            delete individual.IndividualType;
            individual.agentId = individual.agentId.substring(agentIdPrefix.length);
            individual.lineageGuids = [];
            individual.parentGuids.forEach((parentGuid) => {
                individual.lineageGuids.push(parentGuid);
                if(generationIndex > 0){
                    input.generations[generationIndex-1].forEach((prevGenIndividual) => {
                        if(prevGenIndividual.guid == parentGuid){
                            prevGenIndividual.lineageGuids.forEach((lineageGuid) => {
                                if(!individual.lineageGuids.includes(lineageGuid)){
                                    individual.lineageGuids.push(lineageGuid);
                                }
                            });
                        }
                    });
                }
            });
            return individual;
        });
    });
    return output;
}