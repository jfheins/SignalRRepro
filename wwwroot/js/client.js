"use strict";

function beBusy() {
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

//Disable the send button until connection is established.
document.getElementById("status").innerText = "Preparing...";

const start = performance.now();
let iterations = 0;
while (performance.now() - start < 25) {
  beBusy();
  iterations++;
}

connection.on("clock", function (message) {
  document.getElementById("time").innerText = message.time;
  document.getElementById("delay").innerText = calcDelay(message.time) + " ms";
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
