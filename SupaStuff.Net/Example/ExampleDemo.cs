using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupaStuff.Net;
using SupaStuff.Net.ServerSide;


namespace SupaStuff.Net.Example
{
    public class ExampleDemo
    {
        public bool isRunning = true;
        public Server testServer;
        public ClientSide.Client client;
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
            Console.WriteLine("Initiating Scarry Black Window....");
            
            testServer = new Server(4);
            /*
            testServer.OnClientConnected += (ClientConnection conn) => {
                conn.SendPacket(new ExamplePacket2());
            };
            */
            Console.WriteLine("Starting Server at\n     " + Server.host.ToString() + ":" + Server.port);

            
            Console.ReadKey();

            client = new ClientSide.Client(Server.host);
            Console.WriteLine("Client Started");

            Task task = new Task(updateLoop);
            task.Start();

            client.SendPacket(new ExamplePacket(129));
            client.SendPacket(new ExamplePacket(129));

            Console.ReadKey();

            Console.WriteLine("Closing Server..." + testServer.connections.Count);
            testServer.Dispose();
            isRunning = false;


            Console.ReadKey();


            Console.WriteLine("Completed Successfully");

        }
        public void updateLoop()
        {
            while(isRunning)
            {
                client.Update();
                testServer.Update();
                Task.Delay(50);
            }
        }

        
    }
}
