// Copyright 2011 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See use restrictions at /arcgis/developerkit/userestrictions.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using static System.String;

namespace AgsManager
{
    public static class Program
    {
        //Ctrl+G keyboard Bell for error messages
        private const string ErrorBell = "\x7";
        private const string Build = "v10.1";

        private static void Main(string[] args)
        {
            var conn = new AGSConnectionConfig();

            try
            {
                //print usage if no arguments were passed
                if (args.Length == 0)
                {
                    Usage();
                    return;
                }

                #region determine commands

                string command = null;
                string serverHostname = null;
                var serverUsername = Empty;
                var serverPassword = Empty;
                var serverInstance = "arcgis";
                var severPort = "6080";
                var serviceName = Empty;
                var sType = Empty;
                var sDataParam = Empty;
                var service = new AGSServiceConfig { type = "MapServer", folderName = "/" };

                //look at first arg for server name
                if (args[0].IndexOf(":", StringComparison.Ordinal) < 0 &&
                    args[0].IndexOf("-", StringComparison.Ordinal) < 0)
                {
                    serverHostname = args[0];
                }

                var commandIndex = -1;
                var i = 0;
                foreach (var arg in args)
                {
                    //find the command argument w/ "-"
                    if (arg.StartsWith("-"))
                    {
                        command = arg;
                        commandIndex = i;
                    }

                    //find the credentials
                    if (arg.Split(':').Length > 1)
                    {
                        //username
                        if (string.Equals("USER", arg.Split(':')[0], StringComparison.OrdinalIgnoreCase))
                        {
                            serverUsername = arg.Split(':')[1];
                        }

                        //password
                        if (string.Equals("PWD", arg.Split(':')[0], StringComparison.OrdinalIgnoreCase))
                        {
                            serverPassword = arg.Split(':')[1];
                        }

                        //port
                        if (string.Equals("PORT", arg.Split(':')[0], StringComparison.OrdinalIgnoreCase))
                        {
                            severPort = arg.Split(':')[1];
                        }

                        //instance
                        if (string.Equals("INSTANCE", arg.Split(':')[0], StringComparison.OrdinalIgnoreCase))
                        {
                            serverInstance = arg.Split(':')[1];
                        }
                    }
                    i++;
                }

                //exit if no command was given
                if (commandIndex < 0)
                {
                    Usage();
                    return;
                }

                #region param 

                //get service and folder names


                //08/2012, ensure the search for service name skips other parms with ":" (user, pwd)      
                for (var j = commandIndex + 1; j < args.Length; j++)
                {
                    var seviceNameStr = args[j];
                    if (seviceNameStr.IndexOf(":", StringComparison.Ordinal) < 0)
                    {
                        serviceName = seviceNameStr;

                        var serviceNameParts = serviceName.Split('/');
                        if (serviceNameParts.Length == 1)
                        {
                            service.serviceName = serviceName;
                            break;
                        }
                        service.serviceName = serviceNameParts[1];
                        service.folderName = serviceNameParts[0];
                        break;
                    }
                }


                var typeArg = 2 + commandIndex;

                //11.30.2010, check and see if param is type or optional param for stats, etc:
                //12.10,2010, revised to pickup optional <service> and <minutes> for stats
                if (args.Length > typeArg)
                {
                    if (args[typeArg].IndexOf("Server", StringComparison.CurrentCultureIgnoreCase) > 1)
                    {
                        sType = args[typeArg++];
                        service.type = sType;
                    }
                    else
                    {
                        sDataParam = args[typeArg++];
                    }

                    if (args.Length > typeArg)
                    {
                        sDataParam = args[typeArg];
                    }
                }

                if (IsNullOrWhiteSpace(sType) && (command.IndexOf("-list", StringComparison.Ordinal) < 0 || command.IndexOf("-l", StringComparison.Ordinal) < 0))
                {
                    sType = "MapServer";
                }

                if (serverHostname == null)
                {
                    serverHostname = "localhost";
                }

                #endregion

                #endregion

                //print usage if asking for help
                if ((command == null) || !command.StartsWith("-"))
                {
                    Console.WriteLine(ErrorBell + "\nError: No operation specified!");
                    Usage2(false);
                    return;
                }
                if (command == "-h")
                {
                    Usage2(true);
                    return;
                }

                if (string.Equals("-PAUSE", command, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals("-P", command, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("\nThe PAUSE operation is not available.  Continuing to STOP service.");
                    command = "-STOP";
                }

                #region input params

                conn.server = serverHostname;
                conn.instance = serverInstance;
                conn.port = severPort;
                conn.user = serverUsername;
                conn.password = serverPassword;

                conn.token = GenerateToken(conn);

                if (conn.token == null)
                {
                    Console.WriteLine(ErrorBell + "\nError: Could not connect to server.");
                    Console.WriteLine("\nTips: No token generated with the username and password provided.");
                    Environment.Exit(1);
                    return;
                }

                #endregion

                switch (command)
                {
                    case "-start":
                    case "-s":
                        if (string.Equals("*ALL*", serviceName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("*ALL", serviceName, StringComparison.OrdinalIgnoreCase))
                        {
                            StopStartAll2(conn, ServiceState.Start);
                        }
                        else
                        {
                            Console.WriteLine();
                            if (IsNullOrWhiteSpace(serviceName))
                            {
                                Console.WriteLine(ErrorBell + "Input error: Missing required 'servicename'");
                                Usage();
                            }
                            else
                            {
                                StartService2(conn, service);
                            }
                        }
                        break;

                    case "-stop":
                    case "-x":
                        if (string.Equals("*ALL*", serviceName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("*ALL", serviceName, StringComparison.OrdinalIgnoreCase))
                            StopStartAll2(conn, ServiceState.Stop);
                        else
                        {
                            Console.WriteLine();
                            if (IsNullOrWhiteSpace(serviceName))
                            {
                                Console.WriteLine(ErrorBell + "Input error: Missing required 'servicename'");
                                Usage();
                            }
                            else
                            {
                                StopService2(conn, service);
                            }
                        }

                        break;

                    case "-restart":
                    case "-r":
                        if (string.Equals("*ALL*", serviceName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("*ALL", serviceName, StringComparison.OrdinalIgnoreCase))
                            StopStartAll2(conn, ServiceState.Restart);
                        else
                        {
                            Console.WriteLine();
                            if (IsNullOrWhiteSpace(serviceName))
                            {
                                Console.WriteLine(ErrorBell + "Input error: Missing required 'servicename'");
                                Usage();
                            }
                            else
                            {
                                StopService2(conn, service);
                                StartService2(conn, service);
                            }
                        }
                        break;

                    case "-delete":
                        Console.WriteLine();

                        if (IsNullOrWhiteSpace(serviceName))
                        {
                            Console.WriteLine(ErrorBell + "Input error: Missing required 'servicename'");
                            Usage();
                        }
                        else
                        {
                            if (IsNullOrWhiteSpace(sType))
                            {
                                Console.WriteLine(ErrorBell + "Input error: Missing or invalid 'servicetype'");
                                Usage();
                            }
                            else
                            {
                                DeleteService2(conn, service, sDataParam != "N");
                            }
                        }

                        break;

                    case "-list":
                        Console.WriteLine("\nService Status:\n");

                        #region get configurations

                        var agsConfigs = GetAllConfigurations2(conn);
                        if (agsConfigs == null)
                        {
                            Console.WriteLine("No services could be listed.");
                        }

                        #endregion

                        var count = 0;

                        foreach (var config in agsConfigs)
                        {
                            if (IsNullOrWhiteSpace(serviceName) || (string.Equals(serviceName, config.type, StringComparison.Ordinal)) ||
                                config.serviceName.ToUpper().Contains(serviceName.ToUpper()))
                            {
                                if (IsNullOrWhiteSpace(sType) || string.Equals(config.type, sType, StringComparison.Ordinal))
                                {
                                    var servicePath = ConcatFolderService(config.folderName, config.serviceName);
                                    Console.WriteLine(config.type + " '" + servicePath + "': " + config.status);
                                    count += 1;
                                }
                            }
                        }

                        Console.WriteLine(count == 0 ? "No Service candidates found." : $"\nServices found: {count}");
                        break;

                    case "-listtypes":
                        Console.WriteLine("\nThe Listtypes command is unavailable.");
                        break;
                    case "-stats":
                        Console.WriteLine("\nThe stats command is unavailable.");
                        break;
                    case "-describe":
                        Console.WriteLine("\nService Description(s):");

                        var serviceConfigs = GetAllConfigurations2(conn);

                        count = 0;

                        foreach (var a in serviceConfigs)
                        {
                            if (IsNullOrWhiteSpace(service.serviceName) || string.Equals(service.serviceName, a.type, StringComparison.Ordinal) ||
                                a.serviceName.ToUpper().Contains(service.serviceName.ToUpper()))
                            {
                                if (IsNullOrWhiteSpace(sType) || string.Equals(a.type, sType, StringComparison.OrdinalIgnoreCase))
                                {
                                    DescribeService2(conn, service);

                                    count += 1;
                                }
                            }
                        }

                        Console.WriteLine(count == 0
                                              ? "\nNo Service candidates found."
                                              : $"\nServices found: {count}");
                        break;
                    default:
                        Console.WriteLine($"{ErrorBell}\nInput error: Unknown operation \'{command}\'");
                        Usage();
                        return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ErrorBell + "\nError: " + ex.Message);
                Environment.Exit(1);
            }
        }

        private static string GetServiceStatus(AGSConnectionConfig conn, AGSServiceConfig service)
        {
            try
            {
                var serviceUri = $"http://{conn.server}:{conn.port}/{conn.instance}/admin/services/{ConcatFolderService(service.folderName, service.serviceName)}.{service.type}/status";

                string status;

                using (var wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers[HttpRequestHeader.Accept] = "text/plain";
                    wc.QueryString["f"] = "json";
                    wc.QueryString["token"] = conn.token;

                    var sResults = wc.UploadString(serviceUri, Empty);

                    var byteArray = Encoding.ASCII.GetBytes(sResults);
                    var m = new MemoryStream(byteArray);

                    var ser = new DataContractJsonSerializer(typeof(AGSServiceStatusConfig));

                    var statusobject = (AGSServiceStatusConfig)ser.ReadObject(m);
                    status = statusobject.realTimeState;
                }
                service.status = status;
                return status;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return null;
        }

        private static string GenerateToken(AGSConnectionConfig conn)
        {
            try
            {
                var tokenUri = $"http://{conn.server}:{conn.port}/{conn.instance}/admin/generateToken";

                string token;

                using (var wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers[HttpRequestHeader.Accept] = "text/plain";


                    var reqparm =
                        new NameValueCollection
                        {
                            {"username", conn.user},
                            {"password", conn.password},
                            {"client", "requestip"}
                        };

                    var responsebytes = wc.UploadValues(tokenUri, "POST", reqparm);
                    token = Encoding.UTF8.GetString(responsebytes);
                }

                if (token.IndexOf("<html>", StringComparison.Ordinal) < 0)
                {
                    return token;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return null;
        }

        private static List<AGSServiceConfig> GetAllConfigurations2(AGSConnectionConfig conn)
        {
            try
            {
                var catalogUri = $"http://{conn.server}:{conn.port}/{conn.instance}/admin/services/";
                var list = new List<AGSServiceConfig>();
                using (var wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers[HttpRequestHeader.Accept] = "text/plain";
                    wc.QueryString["f"] = "json";
                    wc.QueryString["token"] = conn.token;

                    var sResults = wc.UploadString(catalogUri, Empty);

                    var byteArray = Encoding.ASCII.GetBytes(sResults);
                    var m = new MemoryStream(byteArray);

                    var ser = new DataContractJsonSerializer(typeof(AGSServiceCatalogConfig));

                    var catalog = (AGSServiceCatalogConfig)ser.ReadObject(m);

                    foreach (var s in catalog.services)
                    {
                        GetServiceStatus(conn, s);
                        list.Add(s);
                    }

                    foreach (var sFolder in catalog.folders)
                    {
                        sResults = wc.UploadString(catalogUri + sFolder, Empty);

                        byteArray = Encoding.ASCII.GetBytes(sResults);
                        var m2 = new MemoryStream(byteArray);

                        ser =
                            new DataContractJsonSerializer(
                                typeof(AGSServiceCatalogConfig));

                        catalog = (AGSServiceCatalogConfig)ser.ReadObject(m2);

                        foreach (var s in catalog.services)
                        {
                            GetServiceStatus(conn, s);
                            list.Add(s);
                        }
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return null;
        }

        private static AGSServiceConfig GetConfiguration(AGSConnectionConfig conn, AGSServiceConfig service)
        {
            try
            {
                var serviceUri = $"http://{conn.server}:{conn.port}/{conn.instance}/admin/services/{ConcatFolderService(service.folderName, service.serviceName)}.{service.type}";

                using (var wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers[HttpRequestHeader.Accept] = "text/plain";
                    wc.QueryString["f"] = "json";
                    wc.QueryString["token"] = conn.token;

                    var sResults = wc.UploadString(serviceUri, Empty);

                    var byteArray = Encoding.ASCII.GetBytes(sResults);
                    var m = new MemoryStream(byteArray);

                    var ser = new DataContractJsonSerializer(typeof(AGSServiceConfig));
                    var s = (AGSServiceConfig)ser.ReadObject(m);
                    GetServiceStatus(conn, service);
                    return s;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return null;
        }

        private static void DescribeService2(AGSConnectionConfig conn, AGSServiceConfig service)
        {
            var s = GetConfiguration(conn, service);

            Console.WriteLine("\nService Name: '" + s.serviceName + "'");
            Console.WriteLine("\tType: " + s.type);
            Console.WriteLine("\tStatus: " + service.status);
            Console.WriteLine("\tDescription: " + s.description);
            Console.WriteLine("\tCapabilities: " + s.capabilities);
            Console.WriteLine("\tCluster Name: " + s.clusterName);
            Console.WriteLine("\tMin Instances Per Node: " + s.minInstancesPerNode);
            Console.WriteLine("\tMax Instances Per Node: " + s.maxInstancesPerNode);
            Console.WriteLine("\tInstances per Container: " + s.instancesPerContainer);
            Console.WriteLine("\tMax Wait Time: " + s.maxWaitTime);
            Console.WriteLine("\tMax Startup Time: " + s.maxStartupTime);
            Console.WriteLine("\tMax Idle Time: " + s.maxIdleTime);
            Console.WriteLine("\tMax Usage Time: " + s.maxUsageTime);
            Console.WriteLine("\tLoad Balancing: " + s.loadBalancing);
            Console.WriteLine("\tIsolation Level: " + s.isolationLevel);
            Console.WriteLine("\tConfigured State: " + s.configuredState);
            Console.WriteLine("\tRecycle Interval: " + s.recycleInterval);
            Console.WriteLine("\tRecycle Start Time: " + s.recycleStartTime);
            Console.WriteLine("\tKeep Alive Interval: " + s.keepAliveInterval);
            Console.WriteLine("\tIsDefault: " + s.isDefault);
            Console.WriteLine("\tExtensions: ");

            foreach (var e in s.extensions)
            {
                Console.WriteLine("\t\ttypeName: " + e.typeName);
                Console.WriteLine("\t\tcapabilities: " + e.capabilities);
                Console.WriteLine("\t\tenabled: " + e.enabled);
                Console.WriteLine("\t\tmaxUploadFileSize: " + e.maxUploadFileSize);
                Console.WriteLine("\t\tallowedUploadFileTypes: " + e.allowedUploadFileTypes);
                Console.WriteLine();
            }
        }

        private static void StopStartAll2(AGSConnectionConfig conn, ServiceState state)
        {
            switch (state)
            {
                case ServiceState.Start:
                    Console.WriteLine("\nAttempting to start *all* stopped services:\n");
                    break;
                case ServiceState.Stop:
                    Console.WriteLine("\nAttempting to stop *all* running services:\n");
                    break;
                case ServiceState.Restart:
                    Console.WriteLine("\nAttempting to restart *all* running services:");
                    break;
                case ServiceState.Pause:
                    Console.WriteLine("\nAttempting to pause *all* running services:\n");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            var count = 0;

            var list = GetAllConfigurations2(conn);


            foreach (var serviceConfig in list)
            {
                switch (state)
                {
                    case ServiceState.Start:
                        if (string.Equals("STOPPED", serviceConfig.status, StringComparison.Ordinal) ||
                            string.Equals("PAUSED", serviceConfig.status, StringComparison.Ordinal))
                        {
                            StartService2(conn, serviceConfig);
                            count++;
                        }
                        break;
                    case ServiceState.Stop:
                        if (string.Equals("STARTED", serviceConfig.status, StringComparison.Ordinal) ||
                            string.Equals("PAUSED", serviceConfig.status, StringComparison.Ordinal))
                        {
                            StopService2(conn, serviceConfig);
                            count++;
                        }
                        break;
                    case ServiceState.Restart:
                        if (string.Equals("STARTED", serviceConfig.status, StringComparison.Ordinal) ||
                            string.Equals("PAUSED", serviceConfig.status, StringComparison.Ordinal))
                        {
                            Console.WriteLine();
                            StopService2(conn, serviceConfig);
                            StartService2(conn, serviceConfig);
                            count++;
                        }
                        break;
                    case ServiceState.Pause:
                        if (string.Equals("STARTED", serviceConfig.status, StringComparison.Ordinal))
                        {
                            Console.WriteLine("PAUSE command is not available.  Continuing to use the STOP command");
                            StopService2(conn, serviceConfig);
                            count++;
                        }
                        break;
                }
            }

            Console.WriteLine(count == 0 ? "\nNo service candidates found." : $"\nServices affected: {count}");
        }

        private static string ConcatFolderService(string folder, string service)
            => folder != "/" ? folder + "/" + service : service;

        private static void StartService2(AGSConnectionConfig conn, AGSServiceConfig service)
        {
            try
            {
                Console.Write("Attempting to start {0} '{1}': ");
                service.status = GetServiceStatus(conn, service);

                if (string.Equals("STOPPED", service.status, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals("PAUSED", service.status, StringComparison.OrdinalIgnoreCase))
                {
                    var serviceUri = $"http://{conn.server}:{conn.port}/{conn.instance}/admin/services/{ConcatFolderService(service.folderName, service.serviceName)}.{service.type + "/start"}";
                    using (var wc = new WebClient())
                    {
                        wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        wc.Headers[HttpRequestHeader.Accept] = "text/plain";
                        wc.QueryString["f"] = "json";
                        wc.QueryString["token"] = conn.token;

                        wc.UploadString(serviceUri, Empty);

                        Console.WriteLine(string.Equals("STARTED", GetServiceStatus(conn, service), StringComparison.OrdinalIgnoreCase)
                                              ? "Successfully started..."
                                              : "Could not be started.");
                    }
                }
                else
                {
                    switch (service.status)
                    {
                        case "DELETED":
                            Console.WriteLine("Can't be started because it was previously deleted.");
                            break;
                        case "STARTED":
                            Console.WriteLine("Is already started.");
                            break;
                        case "STARTING":
                            Console.WriteLine("Can't be started because it is already starting.");
                            break;
                        case "STOPPING":
                            Console.WriteLine("Can't be started because it is currently stopping.");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(ErrorBell + "Error starting service.\n");
                Console.WriteLine(e.Message);
            }
        }

        private static void StopService2(AGSConnectionConfig conn, AGSServiceConfig service)
        {
            try
            {
                Console.Write("Attempting to stop {0} '{1}': ");
                service.status = GetServiceStatus(conn, service);

                if (string.Equals("STARTED", service.status, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals("PAUSED", service.status, StringComparison.OrdinalIgnoreCase))
                {
                    var serviceUri = $"http://{conn.server}:{conn.port}/{conn.instance}/admin/services/{ConcatFolderService(service.folderName, service.serviceName)}.{service.type}/stop";

                    using (var wc = new WebClient())
                    {
                        wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        wc.Headers[HttpRequestHeader.Accept] = "text/plain";
                        wc.QueryString["f"] = "json";
                        wc.QueryString["token"] = conn.token;

                        wc.UploadString(serviceUri, Empty);

                        Console.WriteLine(string.Equals("STOPPED", GetServiceStatus(conn, service), StringComparison.Ordinal)
                                              ? "Successfully stopped..."
                                              : "Could not be stopped.");
                    }
                }
                else
                {
                    switch (service.status)
                    {
                        case "DELETED":
                            Console.WriteLine("Can't be started because it was previously deleted.");
                            break;
                        case "STOPPED":
                            Console.WriteLine("Is already stopped.");
                            break;
                        case "STARTING":
                            Console.WriteLine("Can't be started because it is currently starting.");
                            break;
                        case "STOPPING":
                            Console.WriteLine("Can't be started because it is already stopping.");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(ErrorBell + "Error stopping service.\n");
                Console.WriteLine(e.Message);
            }
        }

        private static void DeleteService2(AGSConnectionConfig conn, AGSServiceConfig service, bool confirm)
        {
            try
            {
                var serviceType = service.type;
                var sName = ConcatFolderService(service.folderName, service.serviceName);
                GetServiceStatus(conn, service);

                while (confirm)
                {
                    Console.Write("Delete '{0}' {1} service, are you sure (yes or no)? ", sName, serviceType);
                    switch (Console.ReadLine()?.ToLower() ?? Empty)
                    {
                        case "yes":
                            confirm = false;
                            Console.WriteLine();
                            break;
                        case "no":
                            Console.WriteLine("\nService deletion CANCELLED!");
                            return;
                    }
                }

                Console.Write("Attempting to delete {0} '{1}': ");

                var sPrefix = "Successfully ";

                if (string.Equals("STARTED", service.status, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals("PAUSED", service.status, StringComparison.OrdinalIgnoreCase))
                {
                    var serviceUri = $"http://{conn.server}:{conn.port}/{conn.instance}/admin/services/{ConcatFolderService(service.folderName, service.serviceName)}.{service.type}/stop";

                    using (var wc = new WebClient())
                    {
                        wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        wc.Headers[HttpRequestHeader.Accept] = "text/plain";
                        wc.QueryString["f"] = "json";
                        wc.QueryString["token"] = conn.token;

                        wc.UploadString(serviceUri, Empty);

                        if (string.Equals("STOPPED", GetServiceStatus(conn, service), StringComparison.Ordinal))
                        {
                            Console.Write(sPrefix + "stopped, ");
                            sPrefix = "and ";
                        }
                        else
                        {
                            Console.WriteLine("Could not be stopped!");
                            Environment.Exit(1);
                        }
                    }
                }

                if (GetServiceStatus(conn, service) == "STOPPED")
                {
                    var serviceUri = $"http://{conn.server}:{conn.port}/{conn.instance}/admin/services/{ConcatFolderService(service.folderName, service.serviceName)}.{service.type}/delete";

                    using (var wc = new WebClient())
                    {
                        wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        wc.Headers[HttpRequestHeader.Accept] = "text/plain";
                        wc.QueryString["f"] = "json";
                        wc.QueryString["token"] = conn.token;

                        wc.UploadString(serviceUri, Empty);
                    }

                    if (IsNullOrWhiteSpace(GetServiceStatus(conn, service)))
                    {
                        //see exception for successful Exception when checking for deleted service
                        Console.WriteLine(sPrefix + "deleted...");
                    }
                    else
                    {
                        Console.WriteLine("Could not be deleted!");
                        Environment.Exit(1);
                    }
                }
                else
                {
                    switch (service.status)
                    {
                        case "DELETED":
                            Console.WriteLine("Can't be deleted because it was previously deleted!");
                            Environment.Exit(0);
                            break;
                        case "STARTING":
                            Console.WriteLine("Can't be deleted because it is currently starting!");
                            Environment.Exit(1);
                            break;
                        case "STOPPING":
                            Console.WriteLine("Can't be deleted because it is currently stopping!");
                            Environment.Exit(1);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(ErrorBell + "Error deleting service!\n");
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }

        private static void Usage() => Usage2(false);

        private static void Usage2(bool help)
        {
            Console.WriteLine("\nAGSSOM " + Build + ", usage:\n");
            Console.WriteLine("AGSSOM -h extended help\n");
            Console.WriteLine("AGSSOM [server] [port:6080] [instance:ArcGIS] user: pwd: {-s | -start}   {[servicename [servicetype]] | *all*}\n");
            Console.WriteLine("AGSSOM [server] [port:6080] [instance:ArcGIS] user: pwd: {-x | -stop}    {[servicename [servicetype]] | *all*}\n");
            Console.WriteLine("AGSSOM [server] [port:6080] [instance:ArcGIS] user: pwd: {-r | -restart} {[servicename [servicetype]] | *all*}\n");
            Console.WriteLine("AGSSOM [server] [port:6080] [instance:ArcGIS] user: pwd: -delete servicename servicetype [N]\n");
            Console.WriteLine("AGSSOM [server] [port:6080] [instance:ArcGIS] user: pwd: -list [likename] [servicetype]\n");
            Console.WriteLine("AGSSOM [server] [port:6080] [instance:ArcGIS] user: pwd: -describe [likename] [servicetype]\n");

            if (help)
            {
                Console.WriteLine("\nOperations:");
                Console.WriteLine("         -s          start a stopped service");
                Console.WriteLine("         -x          stop a started service");
                Console.WriteLine("         -r          restart (stop then start) a started service");
                Console.WriteLine("         -delete     delete or remove a service. First, stop it if running");
                Console.WriteLine("         -describe   describe service details. Default: all services.");
                Console.WriteLine("                     If 'servicetype' omitted, all types will be included.");
                Console.WriteLine("         -list       list status of services. Default: all services.");
                Console.WriteLine("                     If 'servicetype' omitted, all types will be included.");
                Console.WriteLine("         server      local or remote server name. Default: localhost");
                Console.WriteLine("         servicename case sensitive service name");
                Console.WriteLine("         likename    services containing like text in service name");
                Console.WriteLine("         *all*       all services are affected. Trailing asterisk is optional");
                Console.WriteLine("         servicetype case sensitive service type: MapServer(default),");
                Console.WriteLine("                     GeocodeServer, FeatureServer, GeometryServer,");
                Console.WriteLine("                     GlobeServer, GPServer, ImageServer, GeoDataServer");
            }

            Environment.Exit(-1);
        }

        private enum ServiceState
        {
            Start,
            Stop,
            Restart,
            Pause
        }
    }
}