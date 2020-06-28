

if (!String.prototype.supplant) {
    String.prototype.supplant = function (o) {
        return this.replace(/{([^{}]*)}/g,
            function (a, b) {
                var r = o[b];
                return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
    };
}

var forcastTable = document.getElementById('forcastTable');
var forcastTableBody = forcastTable.getElementsByTagName('tbody')[0];
var rowTemplate = '<td>{day}</td><td>{date}</td><td>{temperatureC}</td><td>{temperatureF}</td><td>{forcastType}</td>';
var forcaster = document.getElementById('forcaster');
var forcasterBodyTemplate = forcaster.getElementsByTagName('ul')[0];


let connection = new signalR.HubConnectionBuilder().withUrl("/forcast").build();

connection.start().then(function () {
    connection.invoke("GetAllForcast").then(function (forcasts) {
        for (let i = 0; i < forcasts.length; i++) {
            displayForcast(forcasts[i]);
        }
    });

    connection.invoke("GetState").then(function (state) {
        if (state === 'Running') {
            forcastRunning();
            startStreaming();
        } else {
            forcastClosed();
        }
    });

    document.getElementById('start').onclick = function () {
        connection.invoke("StartForcast");
    }

    document.getElementById('close').onclick = function () {
        connection.invoke("StopForcast");
    }

    document.getElementById('reset').onclick = function () {
        connection.invoke("ResetForcast").then(function () {
            connection.invoke("GetAllForcast").then(function (forcasts) {
                for (let i = 0; i < forcasts.length; i++) {
                    displayForcast(forcasts[i]);
                }
            });
        });
    }
});

connection.on("forcastStarted", function () {
    forcastRunning();
    startStreaming();
});

connection.on("forcastStopped", function () {
    forcastClosed();
});

function startStreaming() {
    connection.stream("StreamForcast").subscribe({
        close: false,
        next: displayForcast,
        error: function (err) {
            logger.log(err);
        }
    });
}

function forcastRunning() {
    document.getElementById('start').setAttribute("disabled", "disabled");
    document.getElementById('close').removeAttribute("disabled");
    document.getElementById('reset').setAttribute("disabled", "disabled");
}

function forcastClosed() {
    document.getElementById('start').removeAttribute("disabled");
    document.getElementById('close').setAttribute("disabled", "disabled");
    document.getElementById('reset').removeAttribute("disabled");
}

function displayForcast(forcast) {
    var displayForcast = formatForcast(forcast);
    addOrReplaceForcast(forcastTableBody, displayForcast, 'tr', rowTemplate);
}

function addOrReplaceForcast(table, forcast, type, template) {
    var child = createForcastNode(forcast, type, template);
    // try to replace
    var forcastNode = document.querySelector(type + "[data-day=" + forcast.day + "]");
    if (forcastNode) {
        table.replaceChild(child, forcastNode);
    } else {
        // add new stock
        table.appendChild(child);
    }
}

function formatForcast(forcast) {
    forcast.temperatureC = forcast.temperatureC.toFixed(2);
    forcast.temperatureF = forcast.temperatureF.toFixed(2);
    

    return forcast;
}

function createForcastNode(forcast, type, template) {
    var child = document.createElement(type);
    child.setAttribute('data-day', forcast.day);
    child.setAttribute('class', forcast.day);
    child.innerHTML = template.supplant(forcast);
    return child;
}