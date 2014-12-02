var solvedSeries = {
};

function toHostArray(type, jsArray) {
    var hostArray = host.newArr(type, jsArray.length);
    jsArray.forEach(function (item, index) {
        hostArray[index] = item;
    });

    return hostArray;
}

function setSolvedValue(seriesDimensions, period, value) {
    var seriesKey = clearSolve.GetVariableKey(toHostArray(String, seriesDimensions));
    
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
    var seriesDimensions = Array.prototype.slice.call(arguments, 0, arguments.length - 1)
        .map(function (arg, i) {
            if (typeof arg == 'string') return arg;
            else return arg.dimensions[i];
        });
    var seriesKey = clearSolve.GetVariableKey(toHostArray(String, seriesDimensions));

    var solvedValue = getSolvedValue(seriesKey, period);
    if (solvedValue != null) return solvedValue;
    else {
        var equation = equations[seriesKey];
        var n = period;
        var s = seriesKey;

        var context = {
            dimensions: seriesDimensions,
            period: period
        };

        var wrappedEquation = String.Format("var {0} = {1}; {2}",
            clearSolve.ContextPlaceholder,
            JSON.stringify(context),
            equation);

        //Console.WriteLine(wrappedEquation);

        var value = eval(wrappedEquation);
        setSolvedValue(seriesDimensions, value);
        return value;
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