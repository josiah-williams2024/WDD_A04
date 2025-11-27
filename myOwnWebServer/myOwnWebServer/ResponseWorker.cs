// FILE               : ResponseWorker.cs
// PROJECT            : myOwnWebServer
// PROGRAMMER		  : Josiah Williams and Ricardo Gao
// FIRST VERSION      : 2025-11-20
// DESCRIPTION        : This file contains the ResponseWorker class
//
// Name               : ResponseWorker.cs
// Purpose            : This is where we https response logic will be handled.
//
// References: NetworkStream: https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream?view=net-10.0
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace myOwnWebServer
{
    internal class ResponseWorker
    {
        TcpClient clientWorker;
        private Logger logger;
        private string webRootWorker;
        private IPAddress serverIPAddress;
        private int serverPort;
        private string httpMethod = "";
        private string httpResource = "";
        private string httpVersion = "";
        int httpStatusCode = 0;

        public ResponseWorker(TcpClient client, string webRoot, Logger logger, IPAddress ipAddress, int port)
        {

            clientWorker = client;
            webRootWorker = webRoot;
            this.logger = logger;
            serverIPAddress = ipAddress;
            serverPort = port;
        }

        public void ProcessRequest()
        {
            NetworkStream stream = clientWorker.GetStream();
            byte[] data = new byte[8192];
            string strRequest;
            int recv = 0;

            try
            {
                IPEndPoint remoteEndPoint = (IPEndPoint)clientWorker.Client.RemoteEndPoint;
                string clientIP = remoteEndPoint.Address.ToString();
                string clientPort = remoteEndPoint.Port.ToString();
                recv = stream.Read(data, 0, data.Length);
                if (recv > 0)
                {
                    strRequest = Encoding.ASCII.GetString(data, 0, recv);
                    logger.LogRequest($"Received request: {strRequest}");
                    ManageRequest(strRequest);
                }

            }
            catch (Exception ex)
            {
                logger.LogError("Error processing request: " + ex.Message);
            }
            finally
            {
                clientWorker.Close();
            }
        }

        public void ManageRequest(string message)
        {
           bool check = ParseRequest(message);
           bool methodCheck = false;
            if (check)
            {
                methodCheck = CheckMethodType(httpMethod);
                //Continue the request management based on method type


            }

        }
        // A method to send response back to client not done just based on example
        public void SendResponse(string responseString)
        {
            try
            {
                NetworkStream stream = clientWorker.GetStream();
                byte[] responseBytes = Encoding.ASCII.GetBytes(responseString);
                stream.Write(responseBytes, 0, responseBytes.Length);
                stream.Flush();
            }
            catch (Exception ex)
            {
                logger.LogError("Error sending response: " + ex.Message);
            }
            finally
            {
                clientWorker.Close();
            }
        }
        /// <summary>
        /// A method to check HTTP method type
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public bool CheckMethodType(string method)
        {
            bool check = false;
            if (method.StartsWith("GET"))
            {
                check = true;
                httpStatusCode = 200;
            }else
            {
                httpStatusCode = 405;
            }
            return check; ;
        }
        /// <summary>
        /// A method to parse HTTP request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool ParseRequest(string request)
        {
            bool check = true;
            string[] separators = new string[] { "\r\n" };
            string[] lineParts = request.Split(separators, StringSplitOptions.None); //Referneced
            if (lineParts.Length == 0) {
                check = false;
            }
            else
            {
                string requestLine = lineParts[0];
                string[] parts = requestLine.Split(' ');
                if (parts.Length == 3)
                {
                    httpMethod = parts[0];
                    httpResource = parts[1];
                    httpVersion = parts[2];
                }
                else
                {
                    check = false;
                }
            }
                return check;
        }
    }
}
