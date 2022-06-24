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
            Main.NetLogger.log("Initiating Scarry Black Window...");
            
            testServer = new Server(4);
            /*
            testServer.OnClientConnected += (ClientConnection conn) => {
                conn.SendPacket(new ExamplePacket2());
            };
            */
            Main.NetLogger.log("Starting Server at\n     " + Server.host.ToString() + ":" + Server.port);

            
            Console.ReadKey();

            client = new ClientSide.Client(Server.host);
            Main.NetLogger.log("Client Started");

            Task task = new Task(updateLoop);
            task.Start();

            Console.ReadKey();

            Console.ReadKey();
            client.SendPacket(new ExamplePacket(129));
            client.SendPacket(new ExamplePacket(129));

            Console.ReadKey();

            Main.NetLogger.log("Closing Server..." + testServer.connections.Count);
            testServer.Dispose();
            isRunning = false;


            Console.ReadKey();


            Main.NetLogger.log("Completed Successfully");

        }
        public void updateLoop()
        {
            while(isRunning)
            {
                client.Update();
                testServer.Update();
                Task.Delay(1000);
            }
        }

        
    }
}
