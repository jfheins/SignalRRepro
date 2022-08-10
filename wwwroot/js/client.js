"use strict";

function beBusy() {
    // Simulates scripting work
    let x = 6171;
    while (x > 1) x = x % 2 ? 3 * x + 1 : x / 2;
}

/**
 * @param {Date} date
 * @return number
 */
function calcDelay(date) {
    return new Date() - new Date(date);
}

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/myHub")
    .withAutomaticReconnect()
    .build();

// Warm up the JIT to make the delay calculation more accurate
for (let i = 0; i < 300; i++) {
    beBusy();
}
document.getElementById("status").innerText = "Ready";

function startClient() {
    document.getElementById("status").innerText = "Preparing...";
    document.getElementById("controls").style.visibility = 'hidden';

    const desiredSlowness = +document.getElementById("slowness").value;
    const start = performance.now();
    let iterations = 0;
    while (performance.now() - start < desiredSlowness) {
        beBusy();
        iterations++;
    }
    console.info(`Running ${ iterations } iterations for ${ desiredSlowness } ms delay`)

    connection.on("clock", function (message) {
        document.getElementById("time").innerText = message.time;
        document.getElementById("id").innerText = message.id;
        document.getElementById("delay").innerText = calcDelay(message.time) + " ms";
        for (let i = 0; i < iterations; i++) {
            beBusy();
        }
    });

    connection
        .start()
        .then((_) => (document.getElementById("status").innerText = "Connected."))
        .catch(function (err) {
            return console.error(err.toString());
        });
}

