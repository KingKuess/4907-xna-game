﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    public class Server
    {
        //Singleton in case we need to access this object without a reference (call <Class_Name>.singleton)
        public static Server singleton;

        //Create an object of the Listener class.
        Listener listener;
        public Listener Listener
        {
            get { return listener; }
        }
        //List of NPCHandlers
        public Dictionary<string, NPCHandler> handlers = new Dictionary<string, NPCHandler>();



        //Array of clients
        Client[] client;

        //number of connected clients
        public int connectedClients = 0;

        //Writers and readers to manipulate data
        MemoryStream readStream;
        MemoryStream writeStream;
        BinaryReader reader;
        BinaryWriter writer;

        /// <summary>
        /// Create a new Server object
        /// </summary>
        /// <param name="port">The port you want to use</param>
        public Server(int port)
        {
            //Initialize the array with a maximum of the MaxClients from t he config file.
            client = new Client[5];

            //Create a new Listener object
            listener = new Listener(port);
            listener.userAdded += new ConnectionEvent(listener_userAdded);
            listener.Start();

            //Create the readers and writers.
            readStream = new MemoryStream();
            writeStream = new MemoryStream();

            reader = new BinaryReader(readStream);
            writer = new BinaryWriter(writeStream);

            //Set the singleton to the current object
            Server.singleton = this;
        }

        internal void askForSync(string map, int mobId)
        {

            foreach (Client c in client)
            {
                if (c != null)
                {
                    if (c.mapName == map)
                    {

                        MemoryStream writeStream = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(writeStream);

                        writeStream.Position = 0;
                        writer.Write((byte)12);
                        writer.Write(Convert.ToInt32(mobId));
                        SendDataToClient(GetDataFromMemoryStream(writeStream), c);
                    }
                }
            }
        }

        /// <summary>
        /// Method that is performed when a new user is added.
        /// </summary>
        /// <param name="sender">The object that sent this message</param>
        /// <param name="user">The user that needs to be added</param>
        private void listener_userAdded(object sender, Client user)
        {
            connectedClients++;

            //Send a message to every other client notifying them on a new client, if the setting is set to True
            if (true)
            {
                writeStream.Position = 0;

                //Write in the form {Protocol}{User_ID}{User_IP}
                writer.Write(1);
                writer.Write(user.id);
                writer.Write(user.IP);

                SendData(GetDataFromMemoryStream(writeStream), user);
            }

            //Set up the events
            user.DataReceived += new DataReceivedEvent(user_DataReceived);
            user.UserDisconnected += new ConnectionEvent(user_UserDisconnected);

            //Print the new player message to the server window.
            Console.WriteLine(user.ToString() + " connected\tConnected Clients:  " + connectedClients + "\n");

            //Add to the client array
            client[user.id] = user;
        }

        /// <summary>
        /// Method that is performed when a new user is disconnected.
        /// </summary>
        /// <param name="sender">The object that sent this message</param>
        /// <param name="user">The user that needs to be disconnected</param>
        private void user_UserDisconnected(object sender, Client user)
        {
            connectedClients--;

            //Send a message to every other client notifying them on a removed client, if the setting is set to True
            if (true)
            {
                writeStream.Position = 0;

                //Write in the form {Protocol}{User_ID}{User_IP}
                writer.Write(0);
                writer.Write(user.id);
                writer.Write(user.IP);

                SendData(GetDataFromMemoryStream(writeStream), user);
            }
            
            //Print the removed player message to the server window.
            Console.WriteLine(user.ToString() + " disconnected\tConnected Clients:  " + connectedClients + "\n");

            

            //Clear the array's index
            client[user.id] = null;
        }

        /// <summary>
        /// Relay messages sent from one client and send them to others
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="data">The data to relay</param>
        private void user_DataReceived(Client sender, byte[] data)
        {
            bool relay = true;
            MemoryStream rstream = new MemoryStream(data);
            BinaryReader r = new BinaryReader(rstream);
            rstream.Position = 0;
            byte command = r.ReadByte();
            if (command == (byte)10)
            {
              
                relay = false;
                string mapname = r.ReadString();
                Console.WriteLine("enemy load command recieved for map: " + mapname);
                sender.mapName = mapname;
                handlers[mapname].active = true;
                sendMobLoad(sender, handlers[mapname].mobs, handlers[mapname].friendlies, mapname);
            }
            else if (command == (byte)12)
            {
                relay = false;
                string mapname = r.ReadString();
                int mobID = r.ReadInt32();
                float X = r.ReadSingle();
                float Y = r.ReadSingle();
                handlers[mapname].mobs[mobID].posX = X;
                handlers[mapname].mobs[mobID].posY = Y;
                handlers[mapname].SendMobMove(mobID);

            }
            else if (command == (byte)7 || command == (byte)8)//enemy hit or died
            {
                int id = r.ReadInt32();
                float x = r.ReadSingle();
                float y = r.ReadSingle();
                int act = r.ReadInt32();

                handlers["World1"].mobs[id].posX = x;
                handlers["World1"].mobs[id].posY = y;
                handlers["World1"].mobs[id].action = act;


            }
            writeStream.Position = 0;

            if (true)
            {
                //Append the id and IP of the original sender to the message, and combine the two data sets.
                writer.Write(sender.id);
                writer.Write(sender.IP);
                data = CombineData(data, writeStream);
            }

            //If we want the original sender to receive the same message it sent, we call a different method
            if (relay)
            {
                SendData(data, sender);
            }
        }

        /// <summary>
        /// Converts a MemoryStream to a byte array
        /// </summary>
        /// <param name="ms">MemoryStream to convert</param>
        /// <returns>Byte array representation of the data</returns>
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

        /// <summary>
        /// Combines one byte array with a MemoryStream
        /// </summary>
        /// <param name="data">Original Message in byte array format</param>
        /// <param name="ms">Message to append to the original message in MemoryStream format</param>
        /// <returns>Combined data in byte array format</returns>
        public byte[] CombineData(byte[] data, MemoryStream ms)
        {
            //Get the byte array from the MemoryStream
            byte[] result = GetDataFromMemoryStream(ms);

            //Create a new array with a size that fits both arrays
            byte[] combinedData = new byte[data.Length + result.Length];

            //Add the original array at the start of the new array
            for (int i = 0; i < data.Length; i++)
            {
                combinedData[i] = data[i];
            }

            //Append the new message at the end of the new array
            for (int j = data.Length; j < data.Length + result.Length; j++)
            {
                combinedData[j] = result[j - data.Length];
            }

            //Return the combined data
            return combinedData;
        }

        /// <summary>
        /// Sends a message to every client except the source.
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="sender">Client that should not receive the message</param>
        private void SendData(byte[] data, Client sender)
        {
            foreach (Client c in client)
            {
                if (c != null && c != sender)
                {
             
                    c.SendData(data);
                }
            }

            //Reset the writestream's position
            writeStream.Position = 0;
        }
        public void SendDataToClient(byte[] data, Client recipient)
        {
            recipient.SendData(data);

            //Reset the writestream's position
            writeStream.Position = 0;
        }

        /// <summary>
        /// Sends data to all clients
        /// </summary>
        /// <param name="data">Data to send</param>
        public void SendData(byte[] data)
        {
            foreach (Client c in client)
            {
                if (c != null)
                    
                    c.SendData(data);
            }

            //Reset the writestream's position
            writeStream.Position = 0;
        }
        public void sendMobLoad(Client recipient, Dictionary<int, NPC> mobs, Dictionary<int, NPC> NPCs, string map)
        {
            foreach (var mob in NPCs)
            {
                mob.Value.writeStream.Position = 0;
                mob.Value.writer.Write((byte)13);
                mob.Value.writer.Write(map);
                mob.Value.writer.Write(Convert.ToInt32(mob.Key));
                mob.Value.writer.Write(mob.Value.texture);
                mob.Value.writer.Write(mob.Value.posX);
                mob.Value.writer.Write(mob.Value.posY);
                mob.Value.writer.Write(Convert.ToInt32(mob.Value.width));
                mob.Value.writer.Write(Convert.ToInt32(mob.Value.height));
                SendDataToClient(GetDataFromMemoryStream(mob.Value.writeStream), recipient);
                Console.WriteLine("Sent NPC: " + mob.Key +" at " + mob.Value.posX + ", " + mob.Value.posY );
                System.Threading.Thread.Sleep(100);
            }
            foreach (var mob in mobs)
            {
                mob.Value.writeStream.Position = 0;
                mob.Value.writer.Write((byte)10);
                mob.Value.writer.Write(map);
                mob.Value.writer.Write(Convert.ToInt32(mob.Key));
                mob.Value.writer.Write(mob.Value.texture);
                mob.Value.writer.Write(Convert.ToInt32(mob.Value.health));
                mob.Value.writer.Write(Convert.ToInt32(mob.Value.strength));
                mob.Value.writer.Write(mob.Value.posX);
                mob.Value.writer.Write(mob.Value.posY);
                mob.Value.writer.Write(Convert.ToInt32(mob.Value.width));
                mob.Value.writer.Write(Convert.ToInt32(mob.Value.height));
                mob.Value.writer.Write(Convert.ToInt32(mob.Value.frames));
                mob.Value.writer.Write(Convert.ToInt32(mob.Value.spawntime));
                mob.Value.writer.Write(mob.Value.isPassive);
                mob.Value.writer.Write(Convert.ToInt32(mob.Value.action));
                SendDataToClient(GetDataFromMemoryStream(mob.Value.writeStream), recipient);
                Console.WriteLine("Sent Mob: " + mob.Key + " at " + mob.Value.posX + ", " + mob.Value.posY);
                System.Threading.Thread.Sleep(100);
               
            }
  
        }
    }
}
