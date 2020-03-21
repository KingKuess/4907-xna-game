using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    public class NPCHandler
    {
        Random rng = new Random();
        string map { get; set; }
        int width;
        int height;

        public Dictionary<int, NPC> mobs;
        public Dictionary<int, NPC> friendlies;
        public bool active;

        Server server { get; set; }




        public NPCHandler(string map, Server server)
        {
          
            active = true;
            this.map = map;
            this.server = server;
            Tuple<int, int> mapSize = Read(map);
            width = mapSize.Item1 * 50;
            height = mapSize.Item2 * 50;
            
            if(map == "World1")
            {
                mobs = new Dictionary<int, NPC>();
                for (int i = 0; i < 10; i++)
                {
                    mobs.Add(i, new NPC("slime", 40, 5, rng.Next(275, width - 275), rng.Next(0, height - 1000), 64, 57, 14, 0, false, false,33));
                }
                for (int i = 10; i < 20; i++)
                {
                    mobs.Add(i, new NPC("zombie", 70, 15, rng.Next(275, width - 275), rng.Next(0, height - 1000), 65, 100, 20, 0, false, false,33));
                }
                mobs.Add(20, new NPC("slimeBoss", 400, 40, 5628, 3199, 146, 131, 14, 0, false, false,33));
                friendlies = new Dictionary<int, NPC>();
                friendlies.Add(21, new NPC("hoodedMan", 0, 0, 8367, 1849, 60, 110, 1, 0, true, true, 0));
            }
            if (map == "Town")
            {
                mobs = new Dictionary<int, NPC>();
                friendlies = new Dictionary<int, NPC>();
                friendlies.Add(1, new NPC("Registrar", 0, 0, 4808, 574, 64, 101, 1, 0, true, true, 0));
                friendlies.Add(2, new NPC("Franks", 0, 0, 9159, 574, 64, 101, 1, 0, true, true, 0));

            }
             server.handlers.Add(map, this);
            active = false;
             while (true)
            {
                if (active)
                {
                 
                    Parallel.ForEach(mobs, (mob) =>
                    {
                        mob.Value.moveTimer--;

                        if (mob.Value.moveTimer <= 0)
                        {
                            server.askForSync(map,mob.Key);
                            mob.Value.moveTimer = 90000;
                        }
                    });



     
                }

            }
        }

        public void SendMobMove(int id)
        {
            NPC mob = mobs[id];
            mob.action = rng.Next(0, 100);

            mob.moveTimer = 50000 + rng.Next(0,40000);
            mob.writeStream.Position = 0;
            mob.writer.Write((byte)6);
            mob.writer.Write(Convert.ToInt32(id));
            mob.writer.Write(mob.posX);
            mob.writer.Write(mob.posY);
            mob.writer.Write(Convert.ToInt32(mob.action));
            server.SendData(GetDataFromMemoryStream(mob.writeStream));
        }

        public Tuple<int,int> Read(string FileName)// this reads in the map from a file
        {
            string[] tempMap = System.IO.File.ReadAllLines(FileName + ".txt");// this fills the tempMap with the rows
            return Tuple.Create<int, int>(tempMap.Length, tempMap[0].Length);
        }
        


        private byte[] GetDataFromMemoryStream(MemoryStream ms)
        {
            byte[] result;

            //Async method called this, so lets lock the object to make sure other threads/async calls need to wait to use it.
            lock (ms)
            {
                int bytesWritten = (int)ms.Position;
                result = new byte[bytesWritten];

                ms.Position = 0;
                ms.Read(result, 0, bytesWritten);
            }

            return result;
        }
    }
    public class NPC
    {
        Random rng = new Random();

        public MemoryStream writeStream;
        public BinaryWriter writer;
        //For enemies
        public string texture;
        public int health;
        public int strength;
        public float posX { get; set; }
        public float posY { get; set; }
        public int width;
        public int height;
        public int frames;
        public int spawntime;
        public bool isPassive;
        public int moveTimer;
        public int action;
        //For NPCs
        public bool isIdle;

        public NPC(string Texture, int Health, int Strength, float PosX, float PosY, int Width, int Height, int Frames, int SpawnTime, bool IsPassive, bool IsIdle, int act)
        {
            writeStream = new MemoryStream();
            writer = new BinaryWriter(writeStream);
            texture = Texture;
            health = Health;
            strength = Strength;
            posX = PosX;
            posY = PosY;
            width = Width;
            height = Height;
            frames = Frames;
            spawntime = SpawnTime;
            isPassive = IsPassive;
            isIdle = IsIdle;
            moveTimer = rng.Next(10000,90000);
            action = act;
        }

    }
}
