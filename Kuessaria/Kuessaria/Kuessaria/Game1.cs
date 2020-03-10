// Kyle Smith, Nathan Esposo, and Andrew Angelone
//2016-06-24
//A Slime Killing RPG that you can level up and play
//Kuessaria  (The title of the game is KuessLand)
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Kuessaria
{

    public class Game1 : Microsoft.Xna.Framework.Game//This is the main Class in which all things are run
    {
        //Networking attributes
        TcpClient client;
        string IP = "127.0.0.1";
        int PORT = 1490;
        int BUFFER_SIZE = 2048;
        byte[] readBuffer;
        MemoryStream readStream, writeStream;
        BinaryReader reader;
        BinaryWriter writer;

        
        GraphicsDeviceManager graphics;//This is literally the graphics device running the game
        SpriteBatch spriteBatch;//This is a variable that enables sprites to be drawn
        enum GameState// I use these enums to differentiate between which "GameState" the game is in using switches, this will affect what loads when
        {
            MainMenu,//the main menu state that will load the menu buttons and background
            LoadCharacter,//the loadcharacter menu that will load all the names of the characters and allow input to select
            Instructions,// the instructions menu that will display the breakdown of the controls and stats
            Playing,//the playing state, where the user has full control to kill slimes
        }
        GameState CurrentGameState = GameState.MainMenu;// this is CurrentGameState that will hold the CurrentGameState
        //Game World
        Random rng = new Random();// this generates a random number generator for use throughout the code
        List<Sprite> platforms = new List<Sprite>();// this is the list that holds all the tiles the character and mobs walk on
        List<Sprite> doors = new List<Sprite>();// These are the sprites the player must be touching to Map switch
        Map map;//This will be used to hold the map tiles and draw them
        Map BackMap;// this is used for the background of the map, loading just like the other map but without collision and a darker set of tiles
        string mapName;//used to determine if a player should load

        //MenuThings
        int SHeight = 1080; int SWidth = 1920;//this is the size of the display, i do not recommend editing these, it can cause things to break
        Buttons BPlay;//This is the play button and will check when its click so it can change state
        Buttons BExit;//This is the exit button and will check when its clicked so it can close the game
        Buttons BInst;//This is the instructions button for displaying the instructions
        SpriteFont font;//This is a font that i use to draw the text on the load character menu
        Texture2D menu1;
        Texture2D menu2;
        Texture2D menu3;
        public string Typed = "";// this is a string that holds the characters the player inputs on the load character menu
        input Load;//this is the input made on the load character menu
        string[] files;//this array holds all the names of the files, for displaying on the load character menu

        //Camera
        Camera Camera;//This is the camera that follows the player as it moves around the world
        //Enemy
        Dictionary<int, Enemy> EnemyList = new Dictionary<int, Enemy>();//This is the list that holds all the slimes in the game

        //Friendly Players
        Dictionary<int, EntitySprite> friendlyPlayers = new Dictionary<int, EntitySprite>();

        //Weapons
        List<Weapon> weapons = new List<Weapon>();

        //player sprites
        PlayerStats Player;//This is the main character sprite, that holds the players stats and positions
        const int Width = 72;//This is the const I made for easy editing of the Player sprites Width
        const int Height = 100;// The same as the last but for height
        SpriteFont PlayerFont;//This is the font i use for the menu in the top left and the Health above the slimes
        Weapon sword;// This is the sword the character weilds, used for collision detection against the mobs to ensue damage

        ////Background Sprites
        Background Scrolling1;// this is the background that follows the character

        //Keyboard
        KeyboardState PastKey;//this is used to determine if a key is still being held so that it doesnt press it multiple times


        public Game1()// this is where the games graphics and devices and key settings load
        {
            graphics = new GraphicsDeviceManager(this);//this sets the graphics to the graphics device the game is using
            //Display
            graphics.PreferredBackBufferHeight = SHeight;//this ensures the game runs at 1920 * 1080
            graphics.PreferredBackBufferWidth = SWidth;// ditto
            //Mouse
            IsMouseVisible = true;// makes the cursor visible
            Content.RootDirectory = "Content";//sets the root directory for all content that will be loaded
        }



        protected override void Initialize()// this is where variable come to be initialized
        {
            Camera = new Camera(GraphicsDevice.Viewport);//this initializes the camera, further explanation in the camera class
            map = new Map();// this initializes the map
            BackMap = new Map();// this initializes the background map
            mapName = "World1";

            //Server attributes
            //client = new TcpClient();
            //client.NoDelay = true;


            readStream = new MemoryStream();
            reader = new BinaryReader(readStream);

            writeStream = new MemoryStream();
            writer = new BinaryWriter(writeStream);

            base.Initialize();//this initializes the base game
            
        }


        protected override void LoadContent()//this loads in the content for sprites and such
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);//this sets spritebatch to be a new spritebatch using the graphics device

            Scrolling1 = new Background(Content.Load<Texture2D>("Backgrounds/Background"), new Rectangle(graphics.GraphicsDevice.Viewport.X, graphics.GraphicsDevice.Viewport.Y, graphics.GraphicsDevice.Viewport.Width + 960, graphics.GraphicsDevice.Viewport.Height + 540));// this initializes the background that follows the character loading in a texture, and using a rectangle that represents the background

            menu1 = Content.Load<Texture2D>("Stats");
            menu2 = Content.Load<Texture2D>("StatsExtended");
            menu3 = Content.Load<Texture2D>("LevelUp");

            sword = new Weapon(Content.Load<Texture2D>("Sword"), new Rectangle(0, 0, 59, 100), Player);//this initalizes the sword loading in the sword texture and making a rectangle that represents the sword in game

            font = Content.Load<SpriteFont>("Load");//This loads the content for the font
            PlayerFont = Content.Load<SpriteFont>("PlayerName");//this loads the content for the playerfont

            Tiles.Content = Content;// this loads the content for the tiles class
            Load = new input(Keyboard.GetState(), Keyboard.GetState());// this loads the input method for the load menu

            //TODO: Read this file name from the network
            BackMap.Generate(BackMap.Read("BackMap.txt"), 75);// this reads in the map and generates it
            map.Generate(map.Read("Map.txt"), 75);// this reads in the map and generates it

           

            BPlay = new Buttons(Content.Load<Texture2D>("Buttons/Button1"), graphics.GraphicsDevice, 600, 100);//sets the texture and size of the play button
            BExit = new Buttons(Content.Load<Texture2D>("Buttons/Button2"), graphics.GraphicsDevice, 600, 100);// sets the texture and size of the exit button
            BInst = new Buttons(Content.Load<Texture2D>("Buttons/Button3"), graphics.GraphicsDevice, 600, 100);// sets the texture and size of the instructions button
            BInst.setPOS(new Vector2(630, 490));//sets the position of the 3 buttons
            BExit.setPOS(new Vector2(630, 640));
            BPlay.setPOS(new Vector2(630, 340));
            
        }
       
        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)//this is the update method that runs alot every second and will constantly do anything in here
        {


            //Menu
            switch (CurrentGameState)//this is a switch that relies on currentgamestate
            {
                case GameState.MainMenu://if the gamestate is main menu
                    if (BPlay.isclicked == true) CurrentGameState = GameState.LoadCharacter;//if the play button is clicked it sets the gamestate to loadcharacter
                    BPlay.Update(Mouse.GetState());//updates the button, checking if the mouse is over it
                    if (BExit.isclicked == true)// if the exit button is clicked
                    {
                        this.Exit();// it exits
                    }
                    BExit.Update(Mouse.GetState());// updates the button
                    if (BInst.isclicked == true) CurrentGameState = GameState.Instructions;// if the instructions button is clicked it switches it into the instructions gamestate
                    BInst.Update(Mouse.GetState());// updates the button
                    break;//break to mark end of case
                case GameState.Instructions://instructions game state
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))//if the escape key if pressed
                    {
                        Mouse.SetPosition(0, 0);// set the mouse to the top left
                        CurrentGameState = GameState.MainMenu;// and go back to the main menu
                    }
                    break;// break to mark end of case
                case GameState.LoadCharacter:// if its in the load character menu
                    Typed = Load.InputKey(Keyboard.GetState(), Typed);// sets typed = to the input by the user using the keyboard
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter) && PastKey.IsKeyUp(Keys.Enter))//checks for the enter key, and ensures enter isnt still being held down from something else
                    {

                        if (Typed == "")//if typed was blank
                        {
                            Player = new PlayerStats(Content.Load<Texture2D>("player"), Width, Height, 8);// it will generate a player with the default settings
                            sword = new Weapon(Content.Load<Texture2D>("Sword"), new Rectangle(0, 0, 59, 100), Player);
                            for (int i = 0; i < 20; i++)//this adds 30 slimes to the list of slimes for use in the game
                                EnemyList.Add(i,new Enemy(Content.Load<Texture2D>("zombie"), 30, 5, new Vector2(200 + rng.Next(275, map.Width - 275), rng.Next(0, map.Height - 375)), 65, 100, 20, rng.Next(0, 1000),false));
                            //this loads in the slimes using a texture, health,strength,position, a width, a height, and an amount of time before the slime spawns
                            EnemyList.Add(25, new Enemy(Content.Load<Texture2D>("slimeBoss"), 300, 20, new Vector2(map.Width / 2, map.Height - 375), 146, 131, 14, 0,false));// this loads in the bossslime the same way but with different values
                            client.Connect(IP, PORT);
                            readBuffer = new byte[BUFFER_SIZE];
                            client.GetStream().BeginRead(readBuffer, 0, BUFFER_SIZE, StreamReceived, null);
                            CurrentGameState = GameState.Playing;//sets the gamestate to playing
                        }
                        else// if typed wasnt blank
                        {
                            Player = new PlayerStats(Typed, Content.Load<Texture2D>("player"), Width, Height, 8, out Typed);// makes a player using typed as the name
                            sword = new Weapon(Content.Load<Texture2D>("Sword"), new Rectangle(0, 0, 59, 100), Player);
                            if (Typed != "Invalid")// if typed Isnt equal to invalid
                                for (int i = 0; i < 20; i++)//this adds 30 slimes to the list of slimes for use in the game                                                                 slime is 64, 57
                                    EnemyList.Add(i,new Enemy(Content.Load<Texture2D>("zombie"), 30+10*Player.Level, 5, new Vector2(rng.Next(275, map.Width - 275), rng.Next(0, map.Height - 375)), 65, 100, 20, rng.Next(0, 1000),false));
                            //this loads in the slimes using a texture, health,strength,position, a width, a height, and an amount of time before the slime spawns
                            EnemyList.Add(25,  new Enemy(Content.Load<Texture2D>("slimeBoss"), 300 + 10 * Player.Level, 20, new Vector2(map.Width / 2, map.Height - 375), 146, 131, 14, 0,false));// this loads in the bossslime the same way but with different values



                            client = new TcpClient();
                            client.NoDelay = true;
                            client.Connect(IP, PORT);
                            readBuffer = new byte[BUFFER_SIZE];
                            client.GetStream().BeginRead(readBuffer, 0, BUFFER_SIZE, StreamReceived, null);
                            CurrentGameState = GameState.Playing;// sets the gamestate to playing
                        }
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))// if they press escape, go back to the main menu
                    {

                        Mouse.SetPosition(0, 0);
                        CurrentGameState = GameState.MainMenu;
                    }
                    PastKey = Keyboard.GetState();// sets the pastkey to whatever they pressed so that they cant hold it down
                    break;//break to mark end of case
                case GameState.Playing://gamestate playing
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))// if they press escape go back to the main menu
                    {

                        Player.SaveCharacter();//saves the character
                        client.Close();
                        Mouse.SetPosition(0, 0);//moves the mouse
                        EnemyList.Clear();
                        CurrentGameState = GameState.MainMenu;//goes to the main menu
                    }
                    //Collison
                    foreach (CollisionTiles tile in map.CollisionTiles)// checks for every tile in the maps list of tiles
                    {
                        Player.CollisionTile(tile.Rectangle, map.Width, map.Height);//if the player is touching it
                        foreach (Enemy enemy in EnemyList.Values)// and for every slime in the Slimes list
                        {
                            enemy.CollisionTile(tile.Rectangle, map.Width, map.Height);// if the slime is touching the tiles
                        }
                        foreach (EntitySprite friendly in friendlyPlayers.Values)
                        {
                            friendly.CollisionTile(tile.Rectangle, map.Width, map.Height);
                        }

                    }

                    //Enemy
                    foreach (Enemy slime in EnemyList.Values)//for every slime in the Slimes list
                    {
                        slime.CollisionPlayer(Player);// check if its touching the player
                        slime.Update(gameTime, map, Player);// update the slime
                        slime.SlimeCollision(sword, Player);// and check if a sword is hitting it
                    }


                    //Player Sprite
                    Player.Update(gameTime, Player, client, writer,writeStream);//updates the player sprite
                    sword.update(Mouse.GetState(), GraphicsDevice.Viewport, sword.owner, writer, writeStream, client);// updates the sword

                    //Friendlies a
                    foreach(EntitySprite friendly in friendlyPlayers.Values)
                    {
                        friendly.Update(gameTime);
                    }
                    for (int i = 0; i < weapons.Count;i++)
                    {
                        if (weapons[i].swinging == Weapon.Swinging.Not && !weapons[i].heal)
                        {
                            weapons.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            weapons[i].update();

                        }
                    }

                    //Camera
                    Camera.Update(gameTime, Player, map.Width, map.Height, Scrolling1.rectangle);// updates the camera
                    ////Scrolling
                    Scrolling1.Update(Camera.CENTRE,map.Width, map.Height);//updates the background



                    break;   //break to mark end of case
            }


            base.Update(gameTime);// updates the base game
        }

        protected override void Draw(GameTime gameTime)//this is where all the sprites get drawn
        {

            GraphicsDevice.Clear(Color.White);//it clears the device every time it draws, which is constantly



            switch (CurrentGameState)// the it uses a switch to determine what to draw based on state
            {
                case GameState.MainMenu://if its the main menu
                    spriteBatch.Begin();//This is so that it can begin drawing, and will be used multiple times
                    spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/MainMenu"), new Rectangle(0, 0, SWidth, SHeight), Color.White);// this draws the background
                    BPlay.Draw(spriteBatch);// this draws the buttons
                    BExit.Draw(spriteBatch);
                    BInst.Draw(spriteBatch);
                    spriteBatch.End();//this marks the end of drawing
                    break;//break to mark end of case
                case GameState.Instructions://this is the intructions gamestate
                    spriteBatch.Begin();
                    spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/Instructions"), new Rectangle(0, 0, SWidth, SHeight), Color.White);// it only draws the image containing the instructions
                    spriteBatch.End();
                    break;
                case GameState.LoadCharacter:// while loading character
                    spriteBatch.Begin();
                    spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/LoadMenu"), new Rectangle(0, 0, SWidth, SHeight), Color.White);// it draws the background
                    spriteBatch.DrawString(font, "Type in your character name(Blank will randomize the name):", new Vector2(0, 0), Color.DarkRed);// the text that will always be the same
                    spriteBatch.DrawString(font, "Saved Characters:", new Vector2(0, 100), Color.DarkRed);//font that is always the same
                    files = System.IO.Directory.GetFiles("Saves");//gets all the files in the saves folder found in the debug folder
                    for (int i = 0; i < files.Length; i++)//for every file
                    {
                        files[i] = files[i].Remove(0, 6);//removes the first six characters, to remove the saves/ from the name
                        files[i] = files[i].Remove(files[i].Length - 4, 4);// removes the last for characters to remove the .txt
                        spriteBatch.DrawString(font, files[i], new Vector2(0, 150 + 50 * i), Color.DarkRed);//draws whatever is left
                    }

                    spriteBatch.DrawString(font, Typed, new Vector2(0, 50), Color.DarkRed);//draws whatever the user has Typed


                    spriteBatch.End();
                    break;
                case GameState.Playing:// while playing
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Camera.transform);//this is different because it uses camera.transform so it can follow the camera

                    ////Background
                    Scrolling1.Draw(spriteBatch);//draws the background

                    //platforms
                    BackMap.Draw(spriteBatch, Color.Gray);//draws the backmao
                    map.Draw(spriteBatch, Color.White);//draws the foreground map


                    //Enemy
                    foreach (Enemy enemy in EnemyList.Values)//draws the slimes
                    {
                        enemy.Draw(spriteBatch, PlayerFont);
                    }



                    //Player           
                    Player.Draw(spriteBatch);//draws the player
                    Player.DrawStatsDisplay(spriteBatch, PlayerFont, graphics.GraphicsDevice.Viewport, Keyboard.GetState(), menu1, menu2, menu3, Camera.centrer, Scrolling1.rectangle);//it then draws the stats menu in the top left
                    sword.Draw(spriteBatch, Player);// draws the sword

                    //Friendlies Sprites
                    foreach (EntitySprite friendly in friendlyPlayers.Values)
                    {
                        friendly.Draw(spriteBatch);
                    }
                    foreach (Weapon wep in weapons)
                    {
                        wep.Draw(spriteBatch,wep.owner);
                    }



                    spriteBatch.End();
                    break;
            }
        }
        private void StreamReceived(IAsyncResult ar)
        {
            int bytesRead = 0;
            try
            {
                lock(client.GetStream())
                {
                    bytesRead = client.GetStream().EndRead(ar);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection error");
            }

            if (bytesRead == 0)//Bad response
            {
                client.Close();
                return;
            }

            byte[] data = new byte[bytesRead];
            
            for (int i = 0; i < bytesRead; i++)
            {
                data[i] = readBuffer[i];
            }

            ProcessMessage(data);
            

            client.GetStream().BeginRead(readBuffer, 0, BUFFER_SIZE, StreamReceived, null);
        }

        private void ProcessMessage(byte[] data)
        {
            //reset read stream
            readStream.SetLength(0);
            readStream.Position = 0;

            readStream.Write(data,0,data.Length);
            readStream.Position = 0;

            Protocol p;
            try
            {
                
                p = (Protocol)reader.ReadByte();

                if (p==Protocol.Connected)
                {
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();

                    writeStream.Position = 0;
                    writer.Write((byte)Protocol.Load);
                    writer.Write(Convert.ToInt32(Player.Position.X));
                    writer.Write(Convert.ToInt32(Player.Position.Y));
                    writer.Write(mapName);
                    SendData(GetDataFromMemoryStream(writeStream));
                    

                }
                else if (p == Protocol.Disconnected)
                {
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();
                    if(friendlyPlayers.ContainsKey(id))
                    {
 
                            friendlyPlayers.Remove(id);
                        
                    }
                }
                else if (p == Protocol.MapSwitch)
                {
                 
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    string map = reader.ReadString();
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();

                    if (map == mapName)
                    {
        
                            friendlyPlayers.Add(id, new EntitySprite(Content.Load<Texture2D>("player2"), new Vector2(x, y), Width, Height, 8));
                        
                    }
                    else if (friendlyPlayers.ContainsKey(id))
                    {
                  
                            friendlyPlayers.Remove(id);
                        
                    }
                }
                else if (p == Protocol.Load)
                {
       
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    string map = reader.ReadString();
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();

                    if (map == mapName)
                    {
                 
                        friendlyPlayers.Add(id, new EntitySprite(Content.Load<Texture2D>("player2"), new Vector2(x, y), Width, Height, 8));
                        
                        writeStream.Position = 0;
                        writer.Write((byte)Protocol.MapSwitch);
                        writer.Write(Convert.ToInt32(Player.Position.X));
                        writer.Write(Convert.ToInt32(Player.Position.Y));
                        writer.Write(mapName);
                        SendData(GetDataFromMemoryStream(writeStream));
                    }
                }
                else if (p == Protocol.PlayerMoved)
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    int posX = reader.ReadInt32();
                    int posY = reader.ReadInt32();
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();

                    if (friendlyPlayers.ContainsKey(id))
                    {
                        friendlyPlayers[id].Position = new Vector2(posX, posY);                       
                        friendlyPlayers[id].velocity = new Vector2(x, y);
                    }
                }
                else if (p == Protocol.weaponCreated)
                {
                    int swung = reader.ReadByte();

                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();

                    if (friendlyPlayers.ContainsKey(id))
                    {
                        Weapon wep = new Weapon(Content.Load<Texture2D>("Sword"), new Rectangle(0, 0, 59, 100), friendlyPlayers[id]);
                        if (swung == 0)
                        {
                            wep.SWUNG = Weapon.swung.left;
                            wep.swinging = Weapon.Swinging.Is;

                        }
                        else if(swung == 1)
                        {
                            wep.SWUNG = Weapon.swung.right;
                            wep.swinging = Weapon.Swinging.Is;

                        }
                        else if (swung == 2)
                        {
                            wep.heal = true;
                            wep.cooldown = 100;
                        }
           
                            weapons.Add(wep);
                        
                    }
                }
                else if (p == Protocol.enemyMoved)
                {

                }
                
 

            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error:" + e.Message);
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
        /// Code to actually send the data to the client
        /// </summary>
        /// <param name="b">Data to send</param>
        public void SendData(byte[] b)
        {
            //Try to send the data.  If an exception is thrown, disconnect the client
            try
            {
                lock (client.GetStream())
                {
                    client.GetStream().BeginWrite(b, 0, b.Length, null, null);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error sending message: " + e);
            }
        }
    }
}
