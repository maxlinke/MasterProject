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
        return newText;
    }

    function svgPolyline (lineParent, coords, fill, stroke) {
        const lineCoords = coords.map(coord => { return `${coord.x},${coord.y}`; }).join(" ");
        const newLine = createSvgElement("polyline", lineParent);
        newLine.setAttribute("points", lineCoords);
        newLine.setAttribute("fill", fill);
        newLine.setAttribute("stroke", stroke);
        return newLine;
    }

    let loadedData = undefined;
    let scatterOffsetRandomValues = [];

    const displayLineageMode = "Lineage";
    const displayFitnessMode = "Fitness";
    const displayPercentageMode = "Percentage";
    // TODO add a "custom" metric where one can define a string separated with periods and i look that up in the object
    // that would allow me to get the randomness for example for ttt individuals
    // i would have to check if that metric 
    // a) exists
    // b) is a number
    // put a div next to the string input field though that displays why it can't display the current string
    // according to the checks above
    const svgDisplayModes = [ displayLineageMode, displayFitnessMode ];
    const svgDisplayModeDropdown = document.getElementById("svgDisplayModeSelection");
    function getSvgDisplayMode () { return svgDisplayModeDropdown.value; }

    const popupEnabledModes = [ "Enabled", "Disabled" ];
    const popupEnabledSelection = document.getElementById("svgPopupEnabledSelection");
    function getPopupsEnabled () { return popupEnabledSelection.value == "Enabled"; }

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
    const legendSvg = document.getElementById("colorLegendSvg");
    const svgOverlay = document.getElementById("overviewSvgOverlay");
    let legendInitialized = false;

    function updateSvg () {
        svg.replaceChildren();
        svgOverlay.replaceChildren();
        if(loadedData.generations.length > 0){
            switch(getSvgDisplayMode()){
                case displayLineageMode:
                    drawSvgLineage();
                    break;
                case displayFitnessMode:
                    drawSvgMetric((individual) => { return individual.fitness; });
                    break;
                // case displayWinPercentMode:      // TODO ensure additional dropdown to set up which percentage
                //     drawSvgMetric((individual) => { return individual.
                //     break;
                // case displayDrawPercentMode:

                //     break;
                // case displayLossPercentMode:

                //     break;
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

    function setLegendVisible (shouldBeVisible) {
        if(!shouldBeVisible){
            legendSvg.style = "visibility: hidden; position: absolute;";
            return;
        }
        legendSvg.style = "";
        if(!legendInitialized){
            const leftSidePadding = 70; // from axes
            const textWidth = 100;
            const textHeight = 8;   // copy from css
            const spacing = 8;
            const singleElementHeight = 2 * svgBubbleRadius + 2;    // + 2 to prevent bubble outline from being cut off on top and bottom
            const singleElementWidth = 2 * svgBubbleRadius + spacing + textWidth;

            legendSvg.setAttribute("height", singleElementHeight);
            legendSvg.setAttribute("width", leftSidePadding + (loadedData.individualTypes.length * singleElementWidth) + ((loadedData.individualTypes.length) * spacing));
            const bubbleParent = createSvgElement("g", legendSvg);
            bubbleParent.setAttribute("stroke", "black");
            bubbleParent.setAttribute("stroke-width", 1);
            const textParent = createSvgElement("g", legendSvg);
            loadedData.individualTypes.forEach((individualType, index) => {
                const xOffset = leftSidePadding + (index * (singleElementWidth + spacing));
                const newCircle = svgCircle(bubbleParent, xOffset + svgBubbleRadius, singleElementHeight / 2, svgBubbleRadius);
                newCircle.setAttribute("fill", loadedData.individualTypeColors[individualType]);
                const newLabel = svgText(textParent, individualType, xOffset + (2 * svgBubbleRadius) + spacing, (singleElementHeight + textHeight) / 2, "svgLegendLabel");
            });
            legendInitialized = true;
        }
    }
    
    const svgBubbleRadius = 10;
    const svgGenerationSpacing = 60;

    function getCustomMinAndMaxYAxisMinorLabels (minLabel, maxLabel, rectHeight) {
        return [
            { text: maxLabel, y: svgBubbleRadius },
            { text: minLabel, y: rectHeight - svgBubbleRadius }
        ];
    }

    function setupIndividualTypeBubbleGroups (opacity) {
        const bubbleGroup = createSvgElement("g", svg);
        bubbleGroup.setAttribute("stroke", "black");
        bubbleGroup.setAttribute("stroke-opacity", opacity);
        bubbleGroup.setAttribute("stroke-width", 1);
        const individualTypeGroups = {};
        loadedData.individualTypes.forEach(individualType => {
            const individualTypeGroup = createSvgElement("g", bubbleGroup);
            individualTypeGroup.setAttribute("fill", loadedData.individualTypeColors[individualType]);
            individualTypeGroup.setAttribute("fill-opacity", opacity);
            individualTypeGroups[individualType] = individualTypeGroup;
        });
        return individualTypeGroups;
    }

    function createIndividualPopup (individual, x, y) {
        const newPopup = document.createElement("div");
        svgOverlay.appendChild(newPopup);
        newPopup.innerHTML = individual.popupText;
        newPopup.className = "popup";
        const newPopupStyle = `top: ${y}px; left: ${x}px;`;
        newPopup.style = newPopupStyle;
        return { popup: newPopup, defaultStyle: newPopupStyle };
    }

    function drawSvgLineage () {
        const individualSpacing = 4;
        const defaultOpacity = 0.5;

        const individualCount = loadedData.generations[0].length;
        const bubbleRectWidth = generationIndexToXCoord(svgBubbleRadius * 2, loadedData.generations.length - 1);
        const bubbleRectHeight = (individualCount * 2 * svgBubbleRadius) + ((individualCount - 1) * individualSpacing);
        const minorXLabels = loadedData.generations.map((gen, genIndex) => {
            return { text: genIndex, x: generationIndexToXCoord(0, genIndex) };
        });
        const minorYLabels = getCustomMinAndMaxYAxisMinorLabels("Worst", "Best", bubbleRectHeight);
        const contentOffset = setupSvgSizeDrawAxesAndGetContentOffsets("Generation", "Fitness", bubbleRectWidth, bubbleRectHeight, minorXLabels, minorYLabels);
        setLegendVisible(true);

        const individualTypeGroups = setupIndividualTypeBubbleGroups(defaultOpacity);
        const lineGroup = createSvgElement("g", svg);
        lineGroup.setAttribute("stroke", "black");
        lineGroup.setAttribute("stroke-opacity", defaultOpacity);

        let individualCoords = {};
        let allBubbles = {};
        let allLines = {};
        let allPopups = {};
        loadedData.generations.forEach((generation, genIndex) => {
            const x = generationIndexToXCoord(contentOffset.x, genIndex);
            generation.forEach((individual, individualIndex) => {
                const y = contentOffset.y + svgBubbleRadius + (individualIndex * ((2 * svgBubbleRadius) + individualSpacing));
                const newBubble = svgCircle(individualTypeGroups[individual.individualType], x, y, svgBubbleRadius);
                allBubbles[individual.guid] = newBubble;
                allLines[individual.guid] = [];
                allPopups[individual.guid] = createIndividualPopup(individual, x, y);
                individual.parentGuids.forEach(parentGuid => {
                    const parentXY = individualCoords[parentGuid];
                    const newLine = svgLine(lineGroup, parentXY.x + svgBubbleRadius, parentXY.y, x - svgBubbleRadius, y);
                    allLines[individual.guid].push(newLine);
                });
                individualCoords[individual.guid] = {x: x, y: y};
            });
        });
        loadedData.generations.forEach(generation => {
            generation.forEach(individual => {
                const individualBubble = allBubbles[individual.guid];
                const individualPopup = allPopups[individual.guid];
                const highlightBubbles = [ individualBubble ];
                const highlightLines = [ ...allLines[individual.guid] ];
                individual.lineageGuids.forEach(parentGuid => {
                    highlightBubbles.push(allBubbles[parentGuid]);
                    highlightLines.push(...allLines[parentGuid]);
                });

                individualBubble.onmouseenter = () => {
                    highlightBubbles.forEach(bubble => {
                        bubble.setAttribute("stroke-opacity", 1);
                        bubble.setAttribute("stroke-width", 1.5);
                        bubble.setAttribute("fill-opacity", 1);
                    });
                    highlightLines.forEach(line => {
                        line.setAttribute("stroke-opacity", 1);
                        line.setAttribute("stroke-width", 1.5);
                    });
                    if(getPopupsEnabled()){
                        individualPopup.popup.style = `${individualPopup.defaultStyle} visibility: visible;`;
                    }
                };

                individualBubble.onmouseleave = () => {
                    highlightBubbles.forEach(bubble => {
                        bubble.removeAttribute("stroke-opacity");
                        bubble.removeAttribute("stroke-width");
                        bubble.removeAttribute("fill-opacity");
                    });
                    highlightLines.forEach(line => {
                        line.removeAttribute("stroke-opacity");
                        line.removeAttribute("stroke-width");
                    });
                    individualPopup.popup.style = individualPopup.defaultStyle;
                };
            });
        });

        function generationIndexToXCoord (xOffset, genIndex) {
            return xOffset + svgBubbleRadius + (genIndex * ((2 * svgBubbleRadius) + svgGenerationSpacing));
        }
    }

    function drawSvgMetric (getValueFromIndividual, minMax) {
        if(minMax == undefined){
            minMax = getMinAndMaxValuesFromIndividualMetric(getValueFromIndividual);
        }
        setLegendVisible(true);

        const bubbleRectHeight = 400;
        const bubbleRectWidth = generationIndexToXCoord(svgBubbleRadius * 2, loadedData.generations.length - 1);
        const defaultBubbleOpacity = 0.5;
        
        const minorXLabels = loadedData.generations.map((gen, genIndex) => {
            return { text: genIndex, x: generationIndexToXCoord(0, genIndex) };
        });
        const minorYLabels = getCustomMinAndMaxYAxisMinorLabels(minMax.min.toFixed(2), minMax.max.toFixed(2), bubbleRectHeight);
        const contentOffset = setupSvgSizeDrawAxesAndGetContentOffsets("Generation", getSvgDisplayMode(), bubbleRectWidth, bubbleRectHeight, minorXLabels, minorYLabels);
        const bubbleGroup = createSvgElement("g", svg);
        bubbleGroup.setAttribute("stroke", "black");
        bubbleGroup.setAttribute("stroke-opacity", defaultBubbleOpacity);
        bubbleGroup.setAttribute("stroke-width", 1);
        bubbleGroup.setAttribute("fill-opacity", defaultBubbleOpacity);
        const generationXCoords = [];
        const meanValues = [];
        const medianValues = [];
        const minValues = [];
        const maxValues = [];
        let scatterIndex = 0;
        loadedData.generations.forEach((generation, genIndex) => {
            const x = generationIndexToXCoord(contentOffset.x, genIndex);
            generationXCoords[genIndex] = x;
            const individualsWithValidValues = [];
            const rawValues = [];
            let rawValueSum = 0;
            generation.forEach(individual => {
                const rawValue = Number(getValueFromIndividual(individual));
                if(rawValue != NaN && rawValue != Infinity && rawValue != -Infinity){
                    individualsWithValidValues.push({individual: individual, value: rawValue});
                }
            });
            individualsWithValidValues.sort((a, b) => Math.sign(a.value - b.value));
            individualsWithValidValues.forEach((individualWithValue, i) => {
                const y = valueToY(individualWithValue.value);
                const actualX = x + ((i == 0 || i == (individualsWithValidValues.length - 1)) ? 0 : getDotScatterXOffset());    // first and last are min/max and should be on the vertices of the line
                const newCircle = svgCircle(bubbleGroup, actualX, y, svgBubbleRadius);
                newCircle.setAttribute("fill", loadedData.individualTypeColors[individualWithValue.individual.individualType]);
                rawValues.push(individualWithValue.value);
                rawValueSum += individualWithValue.value;

                const popup = createIndividualPopup(individualWithValue.individual, actualX, y);

                newCircle.onmouseenter = () => {
                    newCircle.setAttribute("stroke-opacity", 1);
                    newCircle.setAttribute("stroke-width", 1.5);
                    newCircle.setAttribute("fill-opacity", 1);
                    if(getPopupsEnabled()){
                        popup.popup.style = `${popup.defaultStyle} visibility: visible;`;
                    }
                };

                newCircle.onmouseleave = () => {
                    newCircle.removeAttribute("stroke-opacity");
                    newCircle.removeAttribute("stroke-width");
                    newCircle.removeAttribute("fill-opacity");
                    popup.popup.style = popup.defaultStyle;
                };

            });
            meanValues.push(rawValueSum / Math.max(1, rawValues.length));
            minValues.push(rawValues[0]);
            maxValues.push(rawValues[rawValues.length - 1]);
            medianValues.push(rawValues[Math.floor(rawValues.length / 2)]);
        });
        const lineParent = createSvgElement("g", svg);
        drawPolyline(minValues.map((val, i) => { return {x: generationXCoords[i], y: valueToY(val)}; }), "Min");
        drawPolyline(maxValues.map((val, i) => { return {x: generationXCoords[i], y: valueToY(val)}; }), "Max");
        drawPolyline(meanValues.map((val, i) => { return {x: generationXCoords[i], y: valueToY(val)}; }), "Mean").setAttribute("stroke-dasharray", 6);
        drawPolyline(medianValues.map((val, i) => { return {x: generationXCoords[i], y: valueToY(val)}; }), "Median").setAttribute("stroke-dasharray", 3);
        // quartiles too?

        function generationIndexToXCoord (xOffset, genIndex) {
            return xOffset + ((svgGenerationSpacing + svgBubbleRadius) * (genIndex + 0.5));
        }

        function getDotScatterXOffset () {
            return (0.25 * Math.pow(scatterOffsetRandomValues[scatterIndex++], 2)) * (svgGenerationSpacing + svgBubbleRadius);
        }

        function valueToY (rawValue) {
            const normedValue = (rawValue - minMax.min) / (minMax.max - minMax.min);
            return contentOffset.y + svgBubbleRadius + ((1 - normedValue) * (bubbleRectHeight - (2 * svgBubbleRadius)));
        }

        function drawPolyline (rawCoords, label) {
            const defaultStrokeWidth = 1;
            const highlightStrokeWidth = 1.5;
            const hoverWidth = 7;
            const mainLine = svgPolyline(lineParent, rawCoords, "none", "black");
            mainLine.setAttribute("stroke-width", defaultStrokeWidth);
            mainLine.style = "pointer-events: none;";
            const hoverLine = svgPolyline(lineParent, rawCoords, "none", "black");
            hoverLine.setAttribute("stroke-width", hoverWidth);
            hoverLine.setAttribute("stroke-opacity", 0);
            const lastCoord = rawCoords[rawCoords.length - 1];
            const newLabel = svgText(lineParent, label, contentOffset.x + bubbleRectWidth + 8, lastCoord.y + (0.5 * 8), "svgMetricDisplayLineLabel");
            hoverLine.onmouseenter = () => { highlightLine(); };
            hoverLine.onmouseleave = () => { resetHighlight(); };
            newLabel.onmouseenter = () => { highlightLine(); };
            newLabel.onmouseleave = () => { resetHighlight(); };
            return mainLine;

            function highlightLine () {
                mainLine.setAttribute("stroke-width", highlightStrokeWidth);
                newLabel.style = "font-weight: bold;";
            }

            function resetHighlight () {
                mainLine.setAttribute("stroke-width", defaultStrokeWidth);
                newLabel.style = "";
            }
        }
    }

    function getMinAndMaxValuesFromIndividualMetric (getValueFromIndividual) {
        const output = { min: Infinity, max: -Infinity };
        loadedData.generations.forEach(generation => {
            generation.forEach(individual => {
                const newValue = Number(getValueFromIndividual(individual));
                if(newValue != NaN && newValue != Infinity && newValue != -Infinity){
                    output.min = Math.min(output.min, newValue);
                    output.max = Math.max(output.max, newValue);
                }
            });
        });
        if(output.min == Infinity && output.max == -Infinity){
            throw new Error("No numbers detected!");
        }
        return output;
    }

// ----- init -----

    initDropdown(svgDisplayModeDropdown, svgDisplayModes, updateSvg);
    initDropdown(popupEnabledSelection, popupEnabledModes);
    // svgDisplayModeDropdown.value = displayFitnessMode;
    loadedData = processBootCampData(input);
    scatterOffsetRandomValues = [];
    loadedData.generations.forEach(gen => {
        gen.forEach(() => {
            scatterOffsetRandomValues.push((2 * Math.random()) - 1);
        });
    });
    console.log(loadedData);
    updateSvg();

}