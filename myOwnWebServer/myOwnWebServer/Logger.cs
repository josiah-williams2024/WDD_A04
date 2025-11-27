// FILE               : logger.cs
// PROJECT            : myOwnWebServer
// PROGRAMMER		  : Josiah Williams and Ricardo Gao
// FIRST VERSION      : 2025-11-20
// DESCRIPTION        : This file contains the Logger class
//
// Name               : Logger.cs
// Purpose            : This is where the logging functionality will be handled.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace myOwnWebServer
{
    internal class Logger
    {
        //Log file name
        string fileName = "myOwnWebServer.log";
        /// <summary>
        /// A method to log message to file with timestamp, type and mesage
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
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
        /// <param name="message"></param>
        public void LogError(string message)
        {
            LogMessage(message, "ERROR");
        }
        /// <summary>
        /// A method to log server start messages
        /// </summary>
        /// <param name="message"></param>
        public void LogServerStart(string message)
        {
            LogMessage(message, "SERVER START");
        }
        /// <summary>
        /// A method to log response stop messages
        /// </summary>
        /// <param name="message"></param>
        public void LogResponse(string message)
        {
            LogMessage(message, "RESPONSE");
        }
        /// <summary>
        /// A method to log request messages
        /// </summary>
        /// <param name="message"></param>
        public void LogRequest(string message)
        {
            LogMessage(message, "REQUEST");
        }
        /// <summary>
        /// A method to log application startup messages
        /// </summary>
        /// <param name="message"></param>
        public void LogStartup(string message)
        {
            LogMessage(message, "APPLICATION STARTUP");
        }
        public void ClearLog()
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                File.WriteAllText(fileName, string.Empty);
            }
            catch (Exception ex)
            {
                //Cant write to console 
            }
        }
    }
}
