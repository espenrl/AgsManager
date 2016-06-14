// Copyright 2008 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See use restrictions at /arcgis/developerkit/userestrictions.

ESRI 
Boston, Indiana, and Redlands offices.

1/2008 

AGSSOM.exe is a sample command line utility for starting, stopping, and restarting ArcGIS Server services. 

It was built with C#.NET. 

Version 1.0, 01/15/08- initial release. 
Version 1.1, 01/22/08- update provided by ESRI Indiana Project Office which: 
* works with remote servers. It will default to localhost so any existing batch file syntax will still work. 
* adds a listtypes command that lists all the server types and their extensions on the machine. 
* expands the describe command. It now lists all properties and enabled extensions and their properties. 
Version 1.2, 02/04/08- update provided by ESRI Indiana Project Office to support the *all* parameter to now work with the -r (restart) and -s (start) as well as the -x (stop) commands. 
Version 2.0 03/10/09 - update includes "Instances Running" and "Instances In Use" status on the Describe command, and a new "Publish" command which will accept a path to an MXD to base a new service from which will be located at the root of the server.  A "PublishAGSResource.bat" is included which could be use as the target of a Windows Send To command to send MXDs directly to AGSSOM.
Version 2.1 02/17/10 - compiled to target "x86" to allow support for 64bit OS
Version 9.3.1 04/13/2010 - QA recompile for ArcGIS 9.3.1
Version 10.0 04/14/2010 - compiled for ArcGIS 10 prerelease
Version 10.0 06/22/2010 - compiled for ArcGIS 10 release
Version 10.1 09/01/2012 - ported to ArcGIS 10.1 release *please see notes below

* The arguments to the utility are:
AGSSOM [usage]: [server] [port:6080] [instance:ArcGIS] user: pwd: [-s|start, -x|stop, -list|list_services -describe|describe [servicename (or *all* for stop, or MXD path for publish)] {servicetype|default:MapServer, GeocodeServer, GPServer, GlobeServer}

* Note that at the 10.1 version of AGSSOM, the administrative user and password arguments are required.

* If omitted the {server} argument will default to localhost. 
* If ommited the {port} argument will default to 6080.
* If ommited the {instance} argument will default to ArcGIS.

You can use the short versions of the commands (ie. "-s, -x, -list", etc.).

The commands provided are: 
START 
STOP 
RESTART 
LIST 
//LISTTYPES (deprecated at AGSSOM 10.1, please see REST Admin API examples)
DESCRIBE 
//PUBLISH (deprecated at AGSSOM 10.1, please see REST Admin API examples)

For all service types other than MapServer, the Service Type parameter must be included. 

Example usage: 
AGSSOM.exe -s USABasemapService user:gisadmin pwd:gisadmin port:6080 instance:ArcGIS
AGSSOM.exe -s USGeocoder1 GeocodeServer user:gisadmin pwd:gisadmin port:6080 instance:ArcGIS
AGSSOM.exe -x USABasemapService user:gisadmin pwd:gisadmin port:6080 instance:ArcGIS
AGSSOM.exe -r USABasemapService user:gisadmin pwd:gisadmin port:6080 instance:ArcGIS
AGSSOM.exe -r USGeocoder1 GeocodeServer user:gisadmin pwd:gisadmin port:6080 instance:ArcGIS
AGSSOM.exe -x *all* user:gisadmin pwd:gisadmin port:6080 instance:ArcGIS
AGSSOM.exe -describe USABasemapService user:gisadmin pwd:gisadmin port:6080 instance:ArcGIS
AGSSOM.exe -list user:gisadmin pwd:gisadmin port:6080 instance:ArcGIS
AGSSOM.exe RemoteGISServer -s USABasemapService user:gisadmin pwd:gisadmin port:6080 instance:ArcGIS

Note: 

By placing the EXE in a folder which is within the System Path variable of Windows, for example, c:\windows\system32, you will be able run the command from the command window directly without needing to specify the full path to the EXE. 

Similiarly, you can also run the command from within a batch file by inserting the command, ie. "AGSSOM.exe -s USABasemapService" in a text file with the extension *.bat. Double clicking the batch file will run AGSSOM.exe with the specified parameters. 




