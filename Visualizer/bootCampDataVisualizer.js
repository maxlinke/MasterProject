'use strict';

function onBootCampDataFileLoaded (input) {

    // https://stackoverflow.com/questions/23588384/dynamically-created-svg-element-not-displaying
    function createSvgElement (tagName, elementParent) { 
        const output = document.createElementNS("http://www.w3.org/2000/svg", tagName);
        if(elementParent != undefined){
            elementParent.appendChild(output);
        }
        return output;
    }

    function svgCircle (circleParent, x, y, r) {
        const newCircle = createSvgElement("circle", circleParent);
        newCircle.setAttribute("cx", x);
        newCircle.setAttribute("cy", y); 
        newCircle.setAttribute("r", r);
        return newCircle;
    }

    function svgLine (lineParent, x1, y1, x2, y2) {
        const newLine = createSvgElement("line", lineParent);
        newLine.setAttribute("x1", x1);
        newLine.setAttribute("y1", y1);
        newLine.setAttribute("x2", x2);
        newLine.setAttribute("y2", y2);
        return newLine;
    }

    let loadedData = undefined;

    const displayLineageMode = "Lineage";
    const displayPerformanceMode = "Performance";
    const svgDisplayModes = [ displayLineageMode, displayPerformanceMode ];
    const svgDisplayModeDropdown = document.getElementById("svgDisplayModeSelection");
    function getSvgDisplayMode () { return svgDisplayModeDropdown.value; }

    // what do i want to visualize?
    // - performance with increasing generations
    //      - point clouds for individual agents
    //      - lines for min/max/average
    //      - x/y plot with x being generations and y being [minFitness, maxFitness]
    //      - since peer/random performance is saved individually, make that showable too
    // - lineage of individuals
    //      - nice orderly grid (still an x/y plot though)
    //      - draw lines between parent(s) and children
    //      - color code individuals based on type
    // - more?
    //      - be able to find all the info about a specific individual when clicking/hovering/whatever
    // i think that's enough for now
    // i could also then add an svg-graph to the tournament vis to show whatever metric we're grouping the agents by as a graph (highly optional though)
    // first make sure the visualization exists
    // then train the g44p agents
    // then i'm done with the "training" part as well
    // and can do chess
    // and godfield

    // i might want to take the prefix processor out into its own thing because this is bound to have the same issues
    // which means, i probably also want to process these files before doing stuff with them here
    // okay, definitely do that
    // shorten the names
    // resolve the individual types from int back to a string
    // add a map from guid to individual to make the cross referencing easier

    // https://www.w3schools.com/tags/tag_svg.asp
    // https://www.w3schools.com/html/html5_svg.asp
    // https://www.w3schools.com/graphics/svg_intro.asp
    // https://stackoverflow.com/questions/19254520/hover-effect-on-svg-group-elements

    // when drawing all the circles and stuff, don't define everything for every circle
    // https://developer.mozilla.org/en-US/docs/Web/SVG/Element/g
    // https://developer.mozilla.org/en-US/docs/Web/SVG/Element/use
    // i think g is enough
    // make a parent group, define the general look
    // then the circles just have their positions

    // arrows: https://developer.mozilla.org/en-US/docs/Web/SVG/Element/marker

    const svg = document.getElementById("overviewSvg");
    svg.style = "border: 1px solid black";  // TODO remove this later

    function updateSvg () {
        svg.replaceChildren();
        if(loadedData.generations.length > 0){
            switch(getSvgDisplayMode()){
                case displayLineageMode:
                    drawSvgLineage();
                    break;
                case displayPerformanceMode:
                    drawSvgPerformance();
                    break;
                default:
                    throw new Error(`Unknown mode ${getSvgDisplayMode()}`);
            }
        }
    }

    // TODO some sort of parameter for marks along the axes
    // at least for the lineage i'd like "best" and "worst"
    function setupSvgSizeDrawAxesAndGetContentOffsets (xAxisLabel, yAxisLabel, contentWidth, contentHeight) {
        console.log("TODO");
        svg.setAttribute("width", contentWidth);
        svg.setAttribute("height", contentHeight);
        return {x: 0, y: 0};
    }

    // potentially add an option to push the generations closer together
    // or only show a subset of generations
    // in case there are a lot of generations..
    function drawSvgLineage () {
        const bubbleRadius = 8;
        const individualSpacing = 4;
        const generationSpacing = 64;
        const defaultOpacity = 0.5;

        const genCount = loadedData.generations.length;
        const individualCount = loadedData.generations[0].length;
        // TODO the axes + labels + a bubble color legend
        const bubbleRectWidth = (genCount * 2 * bubbleRadius) + ((genCount - 1) * generationSpacing);
        const bubbleRectHeight = (individualCount * 2 * bubbleRadius) + ((individualCount - 1) * individualSpacing);
        const contentOffset = setupSvgSizeDrawAxesAndGetContentOffsets("", "Rank", bubbleRectWidth, bubbleRectHeight);

        const bubbleGroup = createSvgElement("g", svg);
        bubbleGroup.setAttribute("stroke", "black");
        bubbleGroup.setAttribute("stroke-opacity", defaultOpacity);
        bubbleGroup.setAttribute("stroke-width", 1);
        const individualTypeGroups = {};
        loadedData.individualTypes.forEach(individualType => {
            const individualTypeGroup = createSvgElement("g", bubbleGroup);
            individualTypeGroup.setAttribute("fill", loadedData.individualTypeColors[individualType]);
            individualTypeGroup.setAttribute("fill-opacity", defaultOpacity);
            individualTypeGroups[individualType] = individualTypeGroup;
        });
        const lineGroup = createSvgElement("g", svg);
        lineGroup.setAttribute("stroke", "black");
        lineGroup.setAttribute("stroke-opacity", defaultOpacity);

        let prevGenIndividualCoords = {};
        let currGenIndividualCoords = {};
        let allBubbles = {};
        let allLines = {};
        loadedData.generations.forEach((generation, genIndex) => {
            const x = contentOffset.x + bubbleRadius + (genIndex * ((2 * bubbleRadius) + generationSpacing));
            generation.forEach((individual, individualIndex) => {
                const y = contentOffset.y + bubbleRadius + (individualIndex * ((2 * bubbleRadius) + individualSpacing));
                const newBubble = svgCircle(individualTypeGroups[individual.individualType], x, y, bubbleRadius);
                allBubbles[individual.guid] = newBubble;
                allLines[individual.guid] = [];
                const highlightBubbles = [ newBubble ];
                const highlightLines = [];
                // TODO actually do the highlighting for the entire lineage
                // this parentguids stuff needs to stay for creating the lines
                // but add a new lineageGuids property in the processor (recursively gotten)
                // and use that for the highlights
                individual.parentGuids.forEach(parentGuid => {
                    const parentXY = prevGenIndividualCoords[parentGuid];
                    const newLine = svgLine(lineGroup, parentXY.x + bubbleRadius, parentXY.y, x - bubbleRadius, y);
                    highlightLines.push(newLine);
                    highlightBubbles.push(allBubbles[parentGuid]);
                    allLines[individual.guid].push(newLine);
                });
                currGenIndividualCoords[individual.guid] = {x: x, y: y};

                newBubble.onmouseenter = () => {
                    highlightBubbles.forEach(bubble => {
                        bubble.setAttribute("stroke-opacity", 1);
                        bubble.setAttribute("fill-opacity", 1);
                    });
                    highlightLines.forEach(line => {
                        line.setAttribute("stroke-opacity", 1);
                    });
                    // TODO info popup with ALL the info
                    // this time i can't just make it a child of the bubble
                    // ...
                    // can i place an invisible, un-raycastable div over the svg?
                    // and put my popups onto that?
                };

                newBubble.onmouseleave = () => {
                    highlightBubbles.forEach(bubble => {
                        bubble.removeAttribute("stroke-opacity");
                        bubble.removeAttribute("fill-opacity");
                    });
                    highlightLines.forEach(line => {
                        line.removeAttribute("stroke-opacity", 1);
                    });
                    // TODO remove the popup
                };
            });
            prevGenIndividualCoords = currGenIndividualCoords;
            currGenIndividualCoords = {};
        });
    }

    // no colors, just dots
    // and polylines for max, min (and avg?) performance
    // the basic axis code should be the same
    // calculate the dimensions you need for what you'll draw
    // call the draw-axes function
    // it scales the svg
    // draws the axes (+ main labels)
    // and this returns the x and y offsets for your content rect
    function drawSvgPerformance () {
        console.log("TODO");
    }

// ----- init -----

    initDropdown(svgDisplayModeDropdown, svgDisplayModes, updateSvg);
    loadedData = processBootCampData(input);
    console.log(loadedData);
    updateSvg();

}