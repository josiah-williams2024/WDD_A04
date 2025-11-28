// FILE               : Server.cs
// PROJECT            : myOwnWebServer
// PROGRAMMER		  : Josiah Williams and Ricardo Gao
// FIRST VERSION      : 2025-11-20
// DESCRIPTION        : This file contains the Server class
//
// Name               : Server.cs
// Purpose            : This is where the server logic will be handled. Listen for connections and hand them off to workers
//
//References: TcpListener: https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=net-10.0
//References  TCPIP Week 9 Windows Desktop Class Lecture Notes: https://conestoga.desire2learn.com/d2l/le/content/1486417/Home

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace myOwnWebServer
{
    internal class Server
    {
        private Logger logger = new Logger();
        private string webRoot = "";
        private string webIP = "";
        private int webPort = 0;
        private IPAddress ipAddress;
        public Server(string root, string IP, int port)
        {
            webRoot = root;
            webIP = IP;
            webPort = port;
        }
        /// <summary>
        /// A method to start the server
        /// </summary>
        public void Start()
        {
            bool validIP = false;
            TcpListener server = null;
            validIP = ConvertIPAddressPort();
            if (validIP)
            {
                try
                {
                    server = new TcpListener(ipAddress, webPort);
                    server.Start();
                    while (true)
                    {
                        TcpClient client = server.AcceptTcpClient();

                        ResponseWorker worker = new ResponseWorker(client, webRoot, logger, ipAddress, webPort);

                        worker.ProcessRequest();

                        client.Close();
                    }
                }
                catch (SocketException se)
                {
                    logger.LogError("SocketException: " + se.Message);
                }
                finally
                {
                    if (server != null)
                    {
                        server.Stop();
                    }
                }
            }
            else
            {
                logger.LogError("Invalid IP Address: " + webIP);
            }

        }
        /// <summary>
        /// A method to convert string IP address to IPAddress type
        /// </summary>
        /// <returns></returns>
        public bool ConvertIPAddressPort()
        {
            string ipString = webIP;
            IPAddress checkIP = null;

            bool check = IPAddress.TryParse(ipString, out checkIP);
            if ((check))
            {
                ipAddress = checkIP;
            }
            return check;
        }
        
    }
}
