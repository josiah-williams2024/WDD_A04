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
//reference'https://learn.microsoft.com/en-us/dotnet/api/system.io.path.combine?view=net-10.0
//reference https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#the-rfc1123-r-r-format-specifier
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace myOwnWebServer
{
    internal class ResponseWorker
    {
        //Class Variables
        TcpClient clientWorker;
        private Logger logger;
        private string webRootWorker;
        private IPAddress serverIPAddress;
        private int serverPort;
        private string httpMethod = "";
        private string httpResource = "";
        private string httpVersion = "";
        int httpStatusCode = 0;

        //Constructor to Pass information that my be needed
        public ResponseWorker(TcpClient client, string webRoot, Logger logger, IPAddress ipAddress, int port)
        {
            clientWorker = client;
            webRootWorker = webRoot;
            this.logger = logger;
            serverIPAddress = ipAddress;
            serverPort = port;
        }
        /// <summary>
        /// A method to Process the request
        /// </summary>
        public void ProcessRequest()
        {

            NetworkStream stream = clientWorker.GetStream();
            byte[] data = new byte[8192];
            string strRequest;
            int recv = 0;

            try
            {
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

        }
        /// <summary>
        /// A method to Send the Error Response
        /// </summary>
        /// <param name="statusCode">Status code for error response</param>
        /// <param name="reasonPhrase">Reason for the error</param>
        public void sendErrorResponse(int statusCode, string reasonPhrase)
        {

            string body = statusCode + " " + reasonPhrase;
            string statusLine = "HTTP/1.1 " + statusCode + " " + reasonPhrase + "\r\n";
            //this is the format for header
            string headers =
                "Content-Type: text/plain\r\n" +
                "Content-Length: " + Encoding.UTF8.GetByteCount(body) + "\r\n" +
                "Server: myOwnWebServer\r\n" +
                "Date: " + DateTime.UtcNow.ToString("R") + "\r\n" +
                "Connection: close\r\n" +
                "\r\n";
            byte[] headerBytes = Encoding.ASCII.GetBytes(statusLine + headers);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

            try
            {//stream to write 
                NetworkStream stream = clientWorker.GetStream();
                stream.Write(headerBytes, 0, headerBytes.Length);
                stream.Write(bodyBytes, 0, bodyBytes.Length);
                stream.Flush();

                // if note resso will write into
                logger.LogResponse(statusCode.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError("Error sending error response: " + ex.Message);
            }
        }
        /// <summary>
        /// A method to manage the request
        /// </summary>
        /// <param name="message">Request message</param>
        public void ManageRequest(string message)
        {
            bool parseOk = ParseRequest(message); //check the request in order

            if (!parseOk) // error return 400
            {
                sendErrorResponse(400, "Bad Request");
                return;
            }

            bool methodOk = CheckMethodType(httpMethod); //check are they GET
            if (!methodOk)
            {
                sendErrorResponse(405, "Method Not Allowed");  // if not get send 405 error
                return;
            }
            try
            {
                string resourcePath = httpResource;  //transfer resource _> local 

                if (resourcePath.StartsWith("/")) // get out ot the /
                {
                    resourcePath = resourcePath.Substring(1);
                }

                string fullPath = Path.Combine(webRootWorker, resourcePath); // give full path

                if (!File.Exists(fullPath))
                {
                    sendErrorResponse(404, "Not Found"); // not exist 404 Error
                    return;
                }

                byte[] bodyBytes = File.ReadAllBytes(fullPath);    // read document

                string mimeType = GetMimeType(fullPath);   //  see the end of the file to get mime type
                if (mimeType == null)
                {
                    sendErrorResponse(404, "Not Found"); // Null mimetype 404 Error
                    return;
                }

                // return header
                string statusLine = httpVersion + " 200 OK\r\n";
                string headers =
                    "Content-Type: " + mimeType + "\r\n" +
                    "Content-Length: " + bodyBytes.Length + "\r\n" +
                    "Server: myOwnWebServer\r\n" +
                    "Date: " + DateTime.UtcNow.ToString("R") + "\r\n" +
                    "Connection: close\r\n" +
                    "\r\n";

                byte[] headerBytes = Encoding.ASCII.GetBytes(statusLine + headers);

                //write back 
                NetworkStream stream = clientWorker.GetStream();
                stream.Write(headerBytes, 0, headerBytes.Length);
                stream.Write(bodyBytes, 0, bodyBytes.Length);
                stream.Flush();

                string logMsg = $"content-type={mimeType}, content-length={bodyBytes.Length}, server=myOwnWebServer, date={DateTime.UtcNow.ToString("R")}";
                logger.LogResponse(logMsg);
            }
            catch (Exception ex)
            {
                // error
                logger.LogError("Error handling request: " + ex.Message);
                sendErrorResponse(500, "Internal Server Error");
            }

        }
        /// <summary>
        /// A method to Get the MimeType
        /// </summary>
        /// <param name="filePath">Filepath to get extension</param>
        /// <returns></returns>
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
        /// <summary>
        /// A method to check HTTP method type for GET 
        /// </summary>
        /// <param name="method">T/F</param>
        /// <returns></returns>
        public bool CheckMethodType(string method)
        {
            bool check = false;
            if (method.StartsWith("GET"))
            {
                check = true;
                httpStatusCode = 200;
            }
            else
            {
                httpStatusCode = 405;
            }
            return check; ;
        }
        /// <summary>
        /// A method to parse HTTP request
        /// </summary>
        /// <param name="request">Request to be parsed</param>
        /// <returns></returns>
        public bool ParseRequest(string request)
        {
            bool check = true;
            string[] separators = new string[] { "\r\n" };
            string[] lineParts = request.Split(separators, StringSplitOptions.None); //Referneced
            if (lineParts.Length == 0)
            {
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
