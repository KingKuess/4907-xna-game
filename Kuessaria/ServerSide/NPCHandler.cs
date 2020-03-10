using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    class NPCHandler
    {
        string map { get; set; }
        Dictionary<int, NPC> mobs;
        Dictionary<int, NPC> friendlies;

        Server server { get; set; }


        MemoryStream writeStream;
        BinaryWriter writer;

        public NPCHandler(string map, Server server)
        {
            writeStream = new MemoryStream();
            writer = new BinaryWriter(writeStream);
            this.map = map;
            this.server = server;
            if(map == "World1")
            {
                writer.Write((byte)6);
            }
        }
    }
    class NPC
    {
        bool isIdle;
        public NPC()
        {

        }
    }
}
