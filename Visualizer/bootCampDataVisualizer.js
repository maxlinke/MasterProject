'use strict';

function onBootCampDataFileLoaded (input) {

    // https://stackoverflow.com/questions/23588384/dynamically-created-svg-element-not-displaying
    function createSvgElement (tagName) { return document.createElementNS("http://www.w3.org/2000/svg", tagName); }

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

    loadedData = processBootCampData(input);
    console.log(loadedData);
    console.log("TODO visualizer");

    const svg = document.getElementById("overviewSvg");

    const w = 500;  // width from number of generations (+ generally required space)
    const h = 300;  // height from number of individuals in generation (+ generally required space)

    svg.setAttribute("width", w);
    svg.setAttribute("height", h);
    svg.style = "background-color: grey";
    
    const circle = createSvgElement("circle");
    circle.setAttribute("cx", w / 2);
    circle.setAttribute("cy", h / 2);
    circle.setAttribute("r", 40);
    circle.setAttribute("stroke", "black");
    circle.setAttribute("stroke-width", 3);
    circle.setAttribute("fill", "red");
    svg.appendChild(circle);

    // the mouse enter/leave stuff works, but not the popup
    // so i'll probably have to make other stuff happen there

    // const newPopup = document.createElement("div");
    // circle.appendChild(newPopup);
    // newPopup.innerHTML = "I AM A POPUP!";
    // newPopup.className = "textPopup";
    // circle.onmouseenter = () => {
    //     newPopup.style = "visibility: visible";
    // };
    // circle.onmouseleave = () => {
    //     newPopup.style = "";
    // }

    circle.onmouseenter = () => { console.log("enter"); };
    circle.onmouseleave = () => { console.log("leave"); };

// ----- init -----

    initDropdown(svgDisplayModeDropdown, svgDisplayModes, console.log);

}