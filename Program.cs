using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;

namespace WOL_AE
{
    [Command(Name = "WOL_AE.exe", Description = "Command line tool which sends WOL magic packet to the specified destination and quit")]
    class Program
    {
        [Argument(0, "mac", "The MAC address to wake up")]
        [Required]
        public string MacAddress { get; }

        [Option("-ip|--ipAddress", Description = "Optional: the IP address to which send the WOL packets (255.255.255.255 by default)")]
        public string IPAddress { get; }
        
        [Option("-p|--port", Description = "Optional: the port to be used when sending WOL packets (9 by default)")]
        public int? PortNumber { get; }

        [Option("-v|--verbose", Description = "Enable verbose logging")]
        public bool Verbose { get; }

        private void OnExecute()
        {
            Console.WriteLine("Starting WOL_AE application with the following arguments:");
            Console.WriteLine($"MacAddress: {MacAddress}");
            Console.WriteLine($"IPAddress: {IPAddress}");
            Console.WriteLine($"PortNumber: {PortNumber}");

            using (WOLClient cl = new WOLClient())
            {
                Console.WriteLine(cl.SendWOL(Verbose, MacAddress, IPAddress, PortNumber)
                                  ? "WOL correctly sent"
                                  : "WOL failed");
            }
        }

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);
    }
}
