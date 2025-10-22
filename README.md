# MES (Manufacturing Execution System)

This is simple manufacturing execution system I'm building for learing purposes. As a PLC programmer, I've been the end user of MES systems before
but I've never created one. The ones I've used have been one of two configurations, either the PLC acts as a server and the MES acts as a client and
regularly polls the PLC, or the MES acts as a server and listens to PLC connections. This application behaves as the later where there is a long-living
server connection for each plc client. For this application, imagine a widget making machine that has six stations that process the widget sequentially
(Station 10 -> Station 20 -> Station 30 -> Station 40 -> Station 50 -> Station 60). At each station the plc checks with the server to see if work can be
performed, performs the work if allow, then updates the server with the status of the part and station related process data.

## What's in the project

### MES
This is a console application that contains the server(s). When the application starts it reads from a configuration file to get the number of stations and what data is 
expected at each station, then starts each server. It also creates the database. The servers run on a worker service.
#### What I think I should add/change:
* Maybe change the way I'm doing the configuration. Originally I was thinking I could have an external config file that
the app reads from to determine the stations and data. The idea was to be flexible so that the code wouldn't have to be
changed for different applications, but I'm not sure how feasible that really is.
* Error handling for server or database issues.

### MES.Common
This is a class library that I used to share items between the projects. It contains models, enums, custom exceptions, configuration files, and classes to validate
configuration.
#### What I think I should add/change:
* As mentioned earlier, I think maybe I should do something different with configuration.
* I used an enum for plc operations, but I'm not sure that is the way to go.

### MES.Data
This is a class library that uses efcore and contains data context and data repository.
#### What I think I should add/change:
* Not sure if I need worry about concurrency.

### MES.PLC
This is a console application that simulates a PLC (Programable Logic Controller). It reads from configuration files and then creates and starts a client for each PLC station. It also generates serial numbers and distributes them to the clients in a FIFO like manner to simulate a serialized widget passing through the produciton line. In a real-world scenario this app either wouldn't exist or it would be significantly different. 
#### What I think I should add/change:
* There's probably a better way to simulate a PLC than the way I did it.
* Figure out a way to simulate station status ex: running, idle, faulted, etc.

### MES.UI
This is a blazor application that provide access to the records in the database. It also provide the user with the ability to change the good/bad status of a widget.
It's pretty plain at the moment.
#### What I think I should add/change:
* I might replace the blazor app with an html/css/js app. I think I'd be better off if I put more time into learning an html/css/js app before using a blazor app.
* Maybe I should move the front end out of the project?

### MES.WebAPI
This is a web api that exposes the database to the front end.
#### What I think I should add/change:
* Security and authorization to control who can access/modify data.
  
## Running the project
Most of my experience has been with visual studio, so I'm not sure what it would take to run the project in vscode. I have the startup configured to launch MES, PLC,
WebAPI, and UI, so you should just have to hit the start button at the top of visual studio. The app will start each project and after a short delay you should see
the messages between the clients and the servers in the console windows.


## Contributing
This project is strictly for my own personal learing purposes. Constructive criticism, tips, or any feedback that could be helpful for me is welcome.
