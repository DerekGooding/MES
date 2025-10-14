# MES (Manufacturing Execution System)

This is simple manufacturing execution system I'm building for learing purposes. As a PLC programmer, I've been the end user of MES systems before,
but I've never created one. The ones I've used have been one of two configurations, either the PLC acts as a server and the MES acts as a client and
regularly polls the PLC, or the MES acts as a server and listens to PLC connections. This application behaves as the later where there is a long-living
server connection for each plc client. For this application, imagine a widget making machine that has six stations that process the widget sequentially
(Station 10 -> Station 20 -> Station 30 -> Station 40 -> Station 50 -> Station 60). At each station the plc checks with the server to see if work can be
performed, performs the work if allow, then updates the server with the status of the part and station related process data.

## Description

An in-depth paragraph about your project and overview of use.

## Getting Started

### Dependencies

* Describe any prerequisites, libraries, OS version, etc., needed before installing program.
* ex. Windows 10

### Installing

* How/where to download your program
* Any modifications needed to be made to files/folders

### Executing program

* How to run the program
* Step-by-step bullets
```
code blocks for commands
```
