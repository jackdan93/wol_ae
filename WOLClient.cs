using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WOL_AE
{
    public class WOLClient : UdpClient
    {
        private const string DEFAULT_BROADCAST_ADDRESS = "255.255.255.255";
        private const int DEFAULT_PORT = 9;

        public WOLClient() : base() { }

        /// <summary>
        /// Send magic packets to wake up the specified MAC_ADDRESS
        /// </summary>
        /// <param name="MAC_ADDRESS">MAC address to wake up</param>
        /// <param name="ipAddr">Optional: the IP address to which send the WOL packets (255.255.255.255 by default)</param>
        /// <param name="portNum">Optional: the port to be used when sending WOL packets (9 by default)</param>
        /// <returns></returns>
        internal bool SendWOL(bool Verbose, string MAC_ADDRESS, string ipAddr = null, int? portNum = null)
        {
            try
            {
                //Parse and validate arguments
                if (Verbose) Console.WriteLine($"Validating MAC address {MAC_ADDRESS}");
                string validatedMACAddress = ParseAndValidateMacAddress(MAC_ADDRESS);
                if (Verbose) Console.WriteLine($"Validated MAC address: {validatedMACAddress}");

                if (Verbose) Console.WriteLine($"Validating IP address {ipAddr}");
                IPAddress validatedIPAddress = ParseAndValidateIPAddress(ipAddr);
                if (Verbose) Console.WriteLine($"Validated IP address: {validatedIPAddress}");

                if (Verbose) Console.WriteLine($"Validating port number {portNum}");
                int validatedPortNumber = ParseAndValidatePortNumber(portNum);
                if (Verbose) Console.WriteLine($"Validated port number: {validatedPortNumber}");

                if (Verbose) Console.WriteLine("Connecting the UDP client to the specified host");
                //Connect the UDP client
                this.Connect(validatedIPAddress, validatedPortNumber);

                if (Verbose) Console.WriteLine("Setting UDP client socket options");
                //Set UDP client socket options (or throw exception if it's still not connected)
                if (!this.Active)
                    throw new WOLClientException("UDP client cannot connect to remote host, aborting...");
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);

                //Buffer to be sent (size is: 6 reserved bytes + MAC bytes length * number of repeat)
                int byteSize = 6 + (6 * 16);
                byte[] bytes = new byte[byteSize];
                int setBytesCounter = 0;

                if (Verbose) Console.WriteLine("Reserving first 6 bytes of magic packet");
                //First 6 bytes must be 0xFF
                for (int y = 0; y < 6; y++)
                    bytes[setBytesCounter++] = 0xFF;

                if (Verbose) Console.WriteLine("Converting 16 copies of MAC address as hexadecimal numbers");
                //Repeat MAC 16 times
                for (int y = 0; y < 16; y++)
                {
                    int i = 0;
                    for (int z = 0; z < 6; z++)
                    {
                        bytes[setBytesCounter++] = byte.Parse(validatedMACAddress.Substring(i, 2), NumberStyles.HexNumber);
                        i += 2;
                    }
                }

                if (Verbose) Console.WriteLine("Sending magic packets to the network");
                //Send wake up packet
                int bytesSent = Send(bytes, byteSize);
                if (Verbose) Console.WriteLine($"Bytes size: {byteSize} | Sent bytes: {bytesSent}");
                return (bytesSent == byteSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine((ex is WOLClientException) ? ex.Message : writeWellFormedException("Unexpected error detected while sending magic packets", ex));
                return false;
            }
        }

        private string ParseAndValidateMacAddress(string MACAddr) 
        {
            //Null check
            if (string.IsNullOrWhiteSpace(MACAddr))
                throw new WOLClientException("MAC ADDRESS not defined! Aborting...");

            //Sanitize MAC_ADDRESS: remove any character which is not in [A-Z0-9]
            if (MACAddr.Any(character => !(char.IsDigit(character) || char.IsLetter(character))))
                MACAddr = string.Join("", MACAddr.Where(character => char.IsDigit(character) || char.IsLetter(character))).Trim();

            //Length check
            if (MACAddr.Length != 12)
                throw new WOLClientException("MAC ADDRESS length is not standard (12 digits --> 6 bytes)! Aborting...");

            return MACAddr;
        }

        private IPAddress ParseAndValidateIPAddress(string ipAddrStr)
        {
            //Null check
            if (string.IsNullOrWhiteSpace(ipAddrStr))
            {
                //Set default broadcast IP
                ipAddrStr = DEFAULT_BROADCAST_ADDRESS;
            }
            else 
            {
                //Check format
                if (ipAddrStr.Where(character => character == '.').Count() != 3 
                    || ipAddrStr.Length < 7 || ipAddrStr.Length > 15)
                    throw new WOLClientException("IP address format is not standard! Aborting...");
            }

            return IPAddress.Parse(ipAddrStr);
        }

        private int ParseAndValidatePortNumber(int? portNum)
        {
            //Null check
            if (!portNum.HasValue)
            {
                //Set default port number
                portNum = DEFAULT_PORT;
            }
            else
            {
                //Check format
                if ((portNum != 7 || portNum != 9) && (portNum < 1024 || portNum > 65353))
                    throw new WOLClientException("Invalid port number specified! Only 7, 9, or a value within the range 1024-65353 can be used! Aborting...");
            }

            return portNum.Value;
        }

        /// <summary>
        /// Prepare well formed exception message
        /// </summary>
        /// <param name="message">Default error message</param>
        /// <param name="ex">Exception object</param>
        /// <returns></returns>
        private string writeWellFormedException(string message, Exception ex)
        {
            message += Environment.NewLine + string.Format("Exception TargetSite: {0}", ex.TargetSite)
                       + Environment.NewLine + string.Format("Exception Message: {0}", ex.Message)
                       + Environment.NewLine + string.Format("Exception StackTrace: {0}", ex.StackTrace);
            if (ex.InnerException != null)
            {
                message += Environment.NewLine + string.Format("Inner exception TargetSite: {0}", ex.InnerException.TargetSite)
                       + Environment.NewLine + string.Format("Inner exception Message: {0}", ex.InnerException.Message)
                       + Environment.NewLine + string.Format("Inner exception StackTrace: {0}", ex.InnerException.StackTrace);
            }
            return message;
        }

        /// <summary>
        /// Exception handler
        /// </summary>
        internal class WOLClientException : Exception
        {
            public WOLClientException() : base() { }

            public WOLClientException(string message) : base(message) { }
        }
    }
}
