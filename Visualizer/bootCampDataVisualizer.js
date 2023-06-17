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

    function svgText (textParent, actualText, x, y, className) {
        const newText = createSvgElement("text", textParent);
        newText.innerHTML = actualText;
        newText.setAttribute("x", x);
        newText.setAttribute("y", y);
        if(className != undefined){
            newText.setAttribute("class", className);
        }
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

    // https://www.w3schools.com/tags/tag_svg.asp
    // https://www.w3schools.com/html/html5_svg.asp
    // https://www.w3schools.com/graphics/svg_intro.asp
    // https://stackoverflow.com/questions/19254520/hover-effect-on-svg-group-elements

    // arrows: https://developer.mozilla.org/en-US/docs/Web/SVG/Element/marker

    const svg = document.getElementById("overviewSvg");
    // svg.style = "border: 1px solid black";  // TODO remove this later

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

    function setupSvgSizeDrawAxesAndGetContentOffsets (xAxisLabel, yAxisLabel, contentWidth, contentHeight, minorXLabels, minorYLabels) {
        const majorLabelFontSizePixels = 8; // copy of the corresponding font size (pt) in the css
        const minorFontSizePixels = 8;      // copy of the corresponding font size (pt) in the css
        const axisWidthIncludingSpace = 16;
        const axisSpace = (axisWidthIncludingSpace - 1) / 2;
        const topSpace = (2 * axisSpace) + majorLabelFontSizePixels;
        const leftSpace = 70;
        const bottomSpace = axisWidthIncludingSpace + minorFontSizePixels + axisSpace;
        const rightSpace = 70;
         
        // the lines
        const lineGroup = createSvgElement("g", svg);
        lineGroup.setAttribute("stroke", "black");
        const yAxisX = leftSpace - axisSpace - 1;
        const xAxisY = topSpace + contentHeight + axisSpace + 1;
        svgLine(lineGroup, yAxisX, topSpace, yAxisX, xAxisY);
        svgLine(lineGroup, yAxisX, xAxisY, yAxisX + contentWidth + axisSpace + 1, xAxisY);
        
        // the labels
        const labelGroup = createSvgElement("g", svg);
        svgText(labelGroup, yAxisLabel, yAxisX, axisSpace + majorLabelFontSizePixels, "svgYAxisMainLabel");
        svgText(labelGroup, xAxisLabel, leftSpace + contentWidth + axisSpace, xAxisY + (majorLabelFontSizePixels / 2) - 1, "svgXAxisMainLabel");
        if(minorXLabels != undefined){
            const labelY = topSpace + contentHeight + bottomSpace - axisSpace;
            minorXLabels.forEach(minorXLabel => {
                svgText(labelGroup, minorXLabel.text, minorXLabel.x + leftSpace, labelY, "svgXAxisMinorLabel");
            });
        }
        if(minorYLabels != undefined){
            const labelX = leftSpace - axisWidthIncludingSpace;
            const yOffset = topSpace + (minorFontSizePixels / 2);
            minorYLabels.forEach(minorYLabel => {
                svgText(labelGroup, minorYLabel.text, labelX, minorYLabel.y + yOffset, "svgYAxisMinorLabel");
            });
        }
        
        svg.setAttribute("width", contentWidth + leftSpace + rightSpace);
        svg.setAttribute("height", contentHeight + topSpace + bottomSpace);
        return {x: leftSpace, y: topSpace};
    }

    // potentially add an option to push the generations closer together
    // or only show a subset of generations
    // in case there are a lot of generations..
    function drawSvgLineage () {
        const bubbleRadius = 8;
        const individualSpacing = 4;
        const generationSpacing = 40;
        const defaultOpacity = 0.5;

        const genCount = loadedData.generations.length;
        const individualCount = loadedData.generations[0].length;
        // TODO the axes + labels + a bubble color legend
        const bubbleRectWidth = (genCount * 2 * bubbleRadius) + ((genCount - 1) * generationSpacing);
        const bubbleRectHeight = (individualCount * 2 * bubbleRadius) + ((individualCount - 1) * individualSpacing);
        const minorXLabels = loadedData.generations.map((gen, genIndex) => {
            return { text: genIndex, x: bubbleRadius + (genIndex * ((2 * bubbleRadius) + generationSpacing)) };
        });
        const minorYLabels = [
            { text: "Best", y: bubbleRadius },
            { text: "Worst", y: bubbleRadius + ((individualCount - 1) * ((2 * bubbleRadius) + individualSpacing)) }
        ];
        const contentOffset = setupSvgSizeDrawAxesAndGetContentOffsets("Generation", "Rank", bubbleRectWidth, bubbleRectHeight, minorXLabels, minorYLabels);

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

        let individualCoords = {};
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
                individual.parentGuids.forEach(parentGuid => {
                    const parentXY = individualCoords[parentGuid];
                    const newLine = svgLine(lineGroup, parentXY.x + bubbleRadius, parentXY.y, x - bubbleRadius, y);
                    highlightLines.push(newLine);
                    allLines[individual.guid].push(newLine);
                });
                individual.lineageGuids.forEach(parentGuid => {
                    allLines[parentGuid].forEach(parentLine => highlightLines.push(parentLine));
                    highlightBubbles.push(allBubbles[parentGuid]);
                });
                individualCoords[individual.guid] = {x: x, y: y};

                newBubble.onmouseenter = () => {
                    highlightBubbles.forEach(bubble => {
                        bubble.setAttribute("stroke-opacity", 1);
                        bubble.setAttribute("stroke-width", 2);
                        bubble.setAttribute("fill-opacity", 1);
                    });
                    highlightLines.forEach(line => {
                        line.setAttribute("stroke-opacity", 1);
                        line.setAttribute("stroke-width", 2);
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
                        bubble.removeAttribute("stroke-width");
                        bubble.removeAttribute("fill-opacity");
                    });
                    highlightLines.forEach(line => {
                        line.removeAttribute("stroke-opacity");
                        line.removeAttribute("stroke-width");
                    });
                    // TODO remove the popup
                };
            });
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