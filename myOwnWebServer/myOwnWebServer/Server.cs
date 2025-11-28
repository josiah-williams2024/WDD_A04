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

                        ResponseWorker worker = new ResponseWorker(client, webRoot, logger,ipAddress,webPort);

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
                    if(server != null)
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
        public void sendRequestTakeResult(object sender,string IpAddress,String IpPort, EventArgs e)
        {    //this will store space for return data
            byte[] data = new byte[8192];
            string strRequest, stringData;
            string errorMessage = "";
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(IpAddress), Convert.ToInt32(IpPort));
            // create socket
            Socket server = new Socket(AddressFamily.InterNetwork,
                                       SocketType.Stream,
                                       ProtocolType.Tcp);

            try
            {
                server.Connect(ipep);
            }
            catch (SocketException ex)
            {
                errorMessage += "Unable to connect to server.";
                errorMessage += ex.ToString();
                errorMessage += IpAddress;
                errorMessage += IpPort;
                Console.WriteLine(errorMessage);
                logger.LogError(errorMessage);
                return;
            }



            //strRequest = txtRequest.Text;
            //server.Send(Encoding.ASCII.GetBytes(strRequest));   // send off the request

            System.Threading.Thread.Sleep(1000);

            int recv = 0;
            while (server.Available > 0)                          // let's read the response and print it out
            {
                recv = server.Receive(data);

                stringData = Encoding.ASCII.GetString(data, 0, recv);

                // check if this is an image being returned ... the HTTPTool doesn't have the ability to 
                // support an image in the RESPONSE window ... so don't encode the returned data into ASCII 
                //   -- instead, output "IMAGE CONTENTS"
                //   -- assuming that the first occurance of the "\r\n\r\n" happens just before the encoded image contents
                //
                int isImage = stringData.IndexOf("Content-Type: image/jpeg");
                if (isImage > 0)
                {
                    // find the \r\n\r\n and cut the string short at that point
                    int imageStart = stringData.IndexOf("\r\n\r\n");
                    //txtReceiveDisplay.Text += stringData.Substring(0, imageStart) + "\r\n\r\n[IMAGE DATA Found Here ...]\r\n";

                }
                else
                {
                    //txtReceiveDisplay.Text += stringData + "\r\n";      // simply add the entire response
                }
            }


            //txtReceiveDisplay.Text += "Disconnecting from server...\r\n";
            server.Shutdown(SocketShutdown.Both);
            server.Close();

        }
    }
}
