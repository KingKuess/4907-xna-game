using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerSide
{
    class Program
    {
        public static Server server;
        static void Main(string[] args)
        {


            //launch listener
            //Try to start a new server using the default port in the config file.
            try
            {
                server = new Server(1490);
                Thread world1Thread = new Thread(new ThreadStart(createWorld1));
                world1Thread.Start();

                Thread townThread = new Thread(new ThreadStart(createTown));
                townThread.Start();
                while (true) { Thread.Sleep(1000); }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.ReadKey();
            //launch Dispatcher

            //launch NPC handler


        }
        public static void createWorld1()
        {
            NPCHandler world1 = new NPCHandler("World1", server);
        }
        public static void createTown()
        {
            NPCHandler Town = new NPCHandler("Town", server);
        }
    }
}
