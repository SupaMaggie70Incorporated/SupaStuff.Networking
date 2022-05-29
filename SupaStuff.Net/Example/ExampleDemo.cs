using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupaStuff.Net;
using SupaStuff.Net.Server;



namespace SupaStuff.Net.Example
{
    public class ExampleDemo
    {
        /// <summary>
        /// Example of how to use SupaStuff.Net
        /// 
        /// Launches local server and client
        /// Prints handshake stage to console
        /// Generates a test packet
        /// Prints to debug console
        /// Adds test packet to queue
        /// Prints when sent and received
        /// Decodes packet and prints
        /// Closes server and client
        /// </summary>

        public ExampleDemo()
        {
            Console.WriteLine("Initiating Scarry Black Window...");
            
            ServerHost testServer = new Server.ServerHost();
            Console.WriteLine("Starting Server at\n     " + ServerHost.host.ToString() + ":" + ServerHost.port);

            
            Console.ReadKey();

            Client.Client client = new Client.Client(ServerHost.host);
            Console.WriteLine("Client Started");

            Console.ReadKey();

            Console.WriteLine("Closing Server...");
            testServer.Dispose();

            Console.WriteLine("Completed Successfully");

        }

        
    }
}
