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
        static void Main(string[] args)
        {
            //launch listener
            //Try to start a new server using the default port in the config file.
            try
            {
                Server server = new Server(1490);

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
    }
}
