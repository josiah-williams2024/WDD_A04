// FILE               : logger.cs
// PROJECT            : myOwnWebServer
// PROGRAMMER		  : Josiah Williams and Ricardo Gao
// FIRST VERSION      : 2025-11-20
// DESCRIPTION        : This file contains the Logger class
//
// Name               : Logger.cs
// Purpose            : This is where the logging functionality will be handled.
using System;
using System.IO;

namespace myOwnWebServer
{
    internal class Logger
    {
        //Class Variable for Log file name
        string fileName = "myOwnWebServer.log";

        /// <summary>
        /// A method to log message to file with timestamp, type and mesage
        /// </summary>
        /// <param name="message">Message to be logged</param>
        /// <param name="type">Type of Log Status</param>
        public void LogMessage(string message, string type)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = timeStamp + " [" + type + "] - " + message + "\r\n";
            try
            {
                File.AppendAllText(fileName, logMessage);
            }
            catch (Exception ex)
            {
                //Cant write to console 
            }
        }
        /// <summary>
        /// A method to log error messages
        /// </summary>
        /// <param name="message">Message to be logged</param>
        public void LogError(string message)
        {
            LogMessage(message, "ERROR"); //Add Type of Log to Message
        }
        /// <summary>
        /// A method to log server start messages
        /// </summary>
        /// <param name="message">Message to be logged</param>
        public void LogServerStart(string message)
        {
            LogMessage(message, "SERVER STARTED"); //Add Type of Log to Messag
        }
        /// <summary>
        /// A method to log response stop messages
        /// </summary>
        /// <param name="message">Message to be logged</param>
        public void LogResponse(string message)
        {
            LogMessage(message, "RESPONSE"); //Add Type of Log to Messag
        }
        /// <summary>
        /// A method to log request messages
        /// </summary>
        /// <param name="message">Message to be logged</param>
        public void LogRequest(string message)
        {
            LogMessage(message, "REQUEST"); //Add Type of Log to Messag
        }
        /// <summary>
        /// A method to clear the log on program startup
        /// </summary>
        public void ClearLog()
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                File.WriteAllText(fileName, string.Empty); //Create empty file
            }
            catch (Exception ex)
            {
                //Cant write to console 
            }
        }
    }
}
