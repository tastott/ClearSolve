var solvedSeries = {
};

function setSolvedValue(seriesDimensions, period, value) {
    var seriesKey = host.GetSeriesKey(seriesDimensions);

    if (!solvedSeries[seriesKey]) solvedSeries[seriesKey] = {};

    solvedSeries[seriesKey][period] = value;
}

function getSolvedValue(seriesKey, period) {
    if (!solvedSeries[seriesKey]) return null;
    else if (solvedSeries[seriesKey][period] == undefined) return null;
    else return solvedSeries[seriesKey][period];
}

function v() {
    //Last argument is period, all others are series dimensions
    var period = arguments[arguments.length - 1];
    var seriesDimensions = Array.prototype.slice.call(arguments, 0, arguments.length - 1);
    var seriesKey = host.GetSeriesKey(seriesDimensions);

    var solvedValue = getSolvedValue(seriesKey, period);
    if (solvedValue != null) return solvedValue;
    else {
        var equation = equations[seriesKey];
        var n = period;
        var s = seriesKey;
        var value = eval(equation);
        setSolvedValue(seriesDimensions, value);
        return value;
    }
}

function getSeriesValues(periods) {
    var values = new Array(periods);

    for (var i = 0; i < periods; i++) {

    }
}

var equations = {};

/*
//Configuration
function makeSeriesKey(dimensions) {
    return dimensions[0] + "¬" + dimensions[1];
}

var equations = {
    "UK¬GDP": "v(UK, GDP, n + 1) * 0.5",
    "US¬GDP": "v(US, GDP, n + 1) + v(UK, GDP, n)"
};

var UK = "UK";
var US = "US";
var GDP = "GDP";


//Solution
setSolvedValue([UK,GDP], 3, 1);
setSolvedValue([US,GDP], 3, 2);

var yValues = [0, 1, 2, 3].map(function (n) {
    return v(US, GDP, n);
});

console.log(yValues);


*/