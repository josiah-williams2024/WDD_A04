// FILE               : MainPorgram.cs
// PROJECT            : myOwnWebServer
// PROGRAMMER		  : Josiah Williams and Ricardo Gao
// FIRST VERSION      : 2025-11-20
// DESCRIPTION        : This file contains the MainProgram class
//
// Name               : MainProgram.cs
// Purpose            : This is where the create the server and parse the command line arguments
//
// References: StartsWith: https://learn.microsoft.com/en-us/dotnet/api/system.string.startswith?view=net-10.0
// References: Substring: https://learn.microsoft.com/en-us/dotnet/api/system.string.substring?view=net-10.0
// References: Dirctoray Exits : https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.exists?view=net-10.0

using System;

namespace myOwnWebServer
{
    internal class MainProgram
    {
       //Class Variables
       private string webRoot = "";
       private string webIP = "";
       private int webPort = 0;
      
        static void Main(string[] args)
        {
            Logger logger = new Logger();
            logger.ClearLog();
           
            MainProgram mainProgram = new MainProgram(); //Need this since ParseArguments is not static and variables are not static

            if (args.Length == 3)
            {
                //Check and parse arguments
                bool checkArgs  = mainProgram.ParseArguments(args);
                bool checkFolder = mainProgram.CheckFolder();
                bool checkPort = mainProgram.CheckPort();
                if (checkArgs)
                {
                    if (checkFolder && checkPort){ //Check if folder exists and port is valid
                        Server server = new Server(mainProgram.webRoot, mainProgram.webIP, mainProgram.webPort);
                        logger.LogServerStart($"WebRoot={mainProgram.webRoot}, WebIP={mainProgram.webIP}, WebPort={mainProgram.webPort}");
                        server.Start();
                    }
                    else
                    {
                        logger.LogError("WebRoot not found or Invalid Port");
                    }
                } else
                {
                    logger.LogError("Invalid format of arguments. Usage Example: myOwnWebServer -webRoot=C:\\localWebSite -webIP=192.168.100.23 -webPort=5300");
                }
            }
            else
            {
                logger.LogError("Invalid number of arguments. Usage Example: myOwnWebServer -webRoot=C:\\localWebSite -webIP=192.168.100.23 -webPort=5300");
            }

        }
        /// <summary>
        /// A method to parse command line arguments
        /// </summary>
        /// <param name="args">Command line agruments to be parsed</param>
        /// <returns></returns>
        public bool ParseArguments(string[] args)
        {
            bool check = true;
            foreach (string arg in args)
            {
                if (arg.StartsWith("-webRoot=")) //Check for webroot
                {
                    string value = arg.Substring("-webRoot=".Length);

                    if (value.Length > 0)
                    {
                        webRoot = value;
                    }
                    else
                    {
                        check = false;
                        break;
                    }
                }
                else if (arg.StartsWith("-webIP=")) //Check for IP
                {
                    string value = arg.Substring("-webIP=".Length);

                    if (value.Length > 0)
                    {
                        webIP = value;
                    }
                    else
                    {
                        check = false;
                        break;
                    }
                }
                else if (arg.StartsWith("-webPort=")) //Check for Port
                {
                    string value = arg.Substring("-webPort=".Length);
                    int port = 0;

                    if (int.TryParse(value, out port))
                    {
                        webPort = port;
                    }
                    else
                    {
                        check = false;
                        break;
                    }
                }
                else
                {
                    check = false;
                    break;
                }
            }

            return check;
        }
        /// <summary>
        /// A method to check if the folder exists
        /// </summary>
        /// <returns></returns>
        public bool CheckFolder()
        {
            bool check = false;
            if (System.IO.Directory.Exists(webRoot))
            {
                check = true;
            }
            return check;
        }
        /// <summary>
        /// A method to Check if the port is valid
        /// </summary>
        /// <returns></returns>
        public bool CheckPort()
        {
            bool check = false;
            if(webPort >= 1 && webPort <= 65535) //Valid port number range
            {
                check = true;
            }
            return check;
        }
    }
}
