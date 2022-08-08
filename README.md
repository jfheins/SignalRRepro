# SignalRRepro
A minimal example of our SignalR memory issues

## Issue summary
When a slow client connects to the app via SignalR we observe the following:
 - The client receives more and more outdated messages
 - The server accumulates memory and continues to do so when the client reconnects
