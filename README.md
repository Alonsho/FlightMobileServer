# FlightMobileServer
Mediator server for mobile app (see [FlightMobileApp](https://github.com/dorgamliel/FlightMobileApp) repository) and [FlightGear](https://www.flightgear.org/download/).

**Description:** mobile app and flightgear both communicate directly with this server. the server processes the communication and passes it to the target. server supports etsablishment of connection, screenshot requests made to FlightGear as well as get/set commands to aircraft controls.

**Running instructions:** make sure the build_and_run script is in the same directory as the project directory (FlightMobileWeb), double click the build_and_run to build and run the server. server address will be shown in concsole. then run FlightGear and the mobile app.
NOTE: server address and port can easily be changed in the appsettings.json file
