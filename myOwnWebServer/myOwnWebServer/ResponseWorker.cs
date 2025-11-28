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
using System.IO;
using System.Runtime.Remoting.Messaging;


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
        public void sendErrorResponse(int statusCode, string reasonPhrase)
        {

            string body = statusCode + " " + reasonPhrase;
            string statusLine = "HTTP/1.1 " + statusCode + " " + reasonPhrase + "\r\n";


            string headers =
                "Content-Type: text/plain\r\n" +
                "Content-Length: " + Encoding.UTF8.GetByteCount(body) + "\r\n" +
                "Server: myOwnWebServer\r\n" +
                "Date: " + DateTime.UtcNow.ToString("R") + "\r\n" +
                "\r\n";
            byte[] headerBytes = Encoding.ASCII.GetBytes(statusLine + headers);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

            try
            {
                NetworkStream stream = clientWorker.GetStream();
                stream.Write(headerBytes, 0, headerBytes.Length);
                stream.Write(bodyBytes, 0, bodyBytes.Length);
                stream.Flush();

                // 按要求：非 200 的响应，日志只写状态码
                logger.LogResponse($"Status={statusCode}");
            }
            catch (Exception ex)
            {
                logger.LogError("Error sending error response: " + ex.Message);
            }
        }
        public void ManageRequest(string message)
        {
            //check the request in order
            bool parseOk = ParseRequest(message);

            if (!parseOk)
            {
                // error return 400
                sendErrorResponse(400, "Bad Request");
                return;
            }

            //check are they GET
            bool methodOk = CheckMethodType(httpMethod);
            if (!methodOk)
            {
                // if not get 
                sendErrorResponse(405, "Method Not Allowed");
                return;
            }
            try
            {
                //transfer resource _> local 
                string resourcePath = httpResource;

                // get out ot the /
                if (resourcePath.StartsWith("/"))
                {
                    resourcePath = resourcePath.Substring(1);
                }

                

                // give full path
                string fullPath = Path.Combine(webRootWorker, resourcePath);

                if (!File.Exists(fullPath))
                {
                    // not exist
                    sendErrorResponse(404, "Not Found");
                    return;
                }

                // read document
                byte[] bodyBytes = File.ReadAllBytes(fullPath);

                //  see the end of the file to get mime type
                string mimeType = GetMimeType(fullPath);

                // return header
                string statusLine = httpVersion + " 200 OK\r\n";
                string headers =
                    "Content-Type: " + mimeType + "\r\n" +
                    "Content-Length: " + bodyBytes.Length + "\r\n" +
                    "Server: myOwnWebServer\r\n" +
                    "Date: " + DateTime.UtcNow.ToString("R") + "\r\n" +
                    "\r\n";

                byte[] headerBytes = Encoding.ASCII.GetBytes(statusLine + headers);

                //write back 
                NetworkStream stream = clientWorker.GetStream();
                stream.Write(headerBytes, 0, headerBytes.Length);
                stream.Write(bodyBytes, 0, bodyBytes.Length);
                stream.Flush();

                //write 200 on header
                logger.LogResponse($"Content-Type={mimeType}; Content-Length={bodyBytes.Length}; Server=myOwnWebServer; Date={DateTime.UtcNow:R}");
            }
            catch (Exception ex)
            {
                // error
                logger.LogError("Error handling request: " + ex.Message);
                sendErrorResponse(500, "Internal Server Error");
            }
        
        }
            private string GetMimeType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            switch (ext)
            {
                case ".html":
                case ".htm":
                    return "text/html";
                case ".txt":
                    return "text/plain";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                default:
                    return null;
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
