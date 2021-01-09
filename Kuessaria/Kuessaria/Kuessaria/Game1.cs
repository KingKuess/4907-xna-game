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
        string IP = "192.168.2.13";
        int PORT = 1490;
        int BUFFER_SIZE = 2048;
        byte[] readBuffer;
        MemoryStream readStream, writeStream;
        BinaryReader reader;
        BinaryWriter writer;
        bool firstLoad = true;

        
        GraphicsDeviceManager graphics;//This is literally the graphics device running the game
        SpriteBatch spriteBatch;//This is a variable that enables sprites to be drawn
        enum GameState// I use these enums to differentiate between which "GameState" the game is in using switches, this will affect what loads when
        {
            MainMenu,//the main menu state that will load the menu buttons and background
            LoadCharacter,//the loadcharacter menu that will load all the names of the characters and allow input to select
            Instructions,// the instructions menu that will display the breakdown of the controls and stats
            Playing,//the playing state, where the user has full control to kill slimes
            MapSwitch,
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

        Buttons ok;
        Buttons close;


        DialogMenu dmenu;
        Texture2D menu1;
        Texture2D menu2;
        Texture2D menu3;
        Texture2D hpBar;
        Texture2D barBack;
        Texture2D manaBar;
        Dictionary<int, Quest> quests = new Dictionary<int, Quest>();

        public string Typed = "";// this is a string that holds the characters the player inputs on the load character menu
        input Load;//this is the input made on the load character menu
        string[] files;//this array holds all the names of the files, for displaying on the load character menu

        //Camera
        Camera Camera;//This is the camera that follows the player as it moves around the world
        //Enemy
        Dictionary<int, Enemy> EnemyList = new Dictionary<int, Enemy>();//This is the list that holds all the slimes in the game

        //Friendly Players
        Dictionary<int, EntitySprite> friendlyPlayers = new Dictionary<int, EntitySprite>();

        //NPC's
        Dictionary<int, EntitySprite> NPCs = new Dictionary<int, EntitySprite>();
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
            mapName = "Town";

            //TESTING THE LENGTH POSSIBLE OF THE DIALOG CHEERS!!!!
            quests.Add(21, new Quest(
            "Psst, hey kid. Don't you get tired of fighting kiddy \n" +
            "slimes. Don't you want to take on something bigger \n" +
            "and badder. This university wont teach you \n" +
            "everything. You have to learn a lot on your own. \n" +
            "Defeat one of those boss slimes in the field. That \n" +
            "will let me know if you're the real deal. ",

            "Huh? Giving up that easy?",

            "Nice job kid, come see me again in your last year\n" +
            "I'll give you a real test!",
            1, 1000));

            quests.Add(1, new Quest(
            "Welcome to battle university. School of the very best \n" +
            "battle warlords. Go to the caves and kill 10 slimes. \n" +
            "This will test your ability against basic enemies \n" +
            "before you can face real challenges. ",

            "You should be able to find them in the caves to the \nleft.",

            "Good job, it seems you are no ordinary vagrant, \n" +
            "perhaps there is hope for you yet to become a \n" +
            "master. Go on and speak to the head professor \n" +
            "Franks. Once again, welcome to battle university,\n" +
            "I hope you don't end up dead and graduate \nsuccessfully. ",
            10, 300));

            quests.Add(2, new Quest(
            "*to himself* Ah, where's my damn snowboard. Oh! A \n" +
            "new student! Welcome to battle university! We take \n" +
            "vagrants like yourself and turn them into great \n" +
            "warlords like me. That is if they don't fall along\n" +
            " the way. The battle isn't against slimes and \n" +
            "zombies only, but also against the monster inside of\n" +
            "oneself. I remember a young promising vagrant who \n" +
            "was able to defeat the toughest of zombies but \n" +
            "sadly lost the battle against the greatest foe, \n" +
            "depression. Ah but I digress. Yes, for your \n" +
            "first lesson, I shall teach you about skills. \n" +
            "Go kill 15 zombies. Return to me when you're \n" +
            "done. Now, where did I leave that damn snowboard. ",

            "Back so soon? If you can't do this... well....",

            "Oho! Ugh, Nope. This is not where I left my \n" +
            "snowboard. Oh, you're back, glad to see you're\n" +
            "not dead yet. *cough cough* (not a corona \n" +
            "virus cough). Don't worry kid, snowboarding \n" +
            "keeps me healthy. Although I haven't found my \n" +
            "snowboard yet. I digress. You've completed my \n" +
            "task! Good job! You've gained enough kill credits\n" +
            " to move onto your second year at battle \n" +
            "university! Congrats! Come back and talk to me \n" +
            "when you're ready to move on. You might consider \n" +
            "taking a break snowboarding or whatever you enjoy!",
            15,500));


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
            hpBar = Content.Load<Texture2D>("hpBar");
            barBack = Content.Load<Texture2D>("barBack");
            manaBar = Content.Load<Texture2D>("manaBar");


            Texture2D dialogBack = Content.Load<Texture2D>("Dialog");

            sword = new Weapon(Content.Load<Texture2D>("Sword"), new Rectangle(0, 0, 59, 100), Player);//this initalizes the sword loading in the sword texture and making a rectangle that represents the sword in game

            font = Content.Load<SpriteFont>("Load");//This loads the content for the font
            PlayerFont = Content.Load<SpriteFont>("PlayerName");//this loads the content for the playerfont

            Tiles.Content = Content;// this loads the content for the tiles class
            Load = new input(Keyboard.GetState(), Keyboard.GetState());// this loads the input method for the load menu

            //TODO: Read this file name from the network
           

            ok = new Buttons(Content.Load<Texture2D>("Buttons/ok"), graphics.GraphicsDevice, 50, 26);
            close = new Buttons(Content.Load<Texture2D>("Buttons/close"), graphics.GraphicsDevice, 50, 26);

            dmenu = new DialogMenu("TESTING THE LENGTH POSSIBLE OF THE DIALOG CHEERS!!!!\n1111111111111111111111", PlayerFont, dialogBack, ok, close);

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

            if (firstLoad)
            {
                Console.WriteLine("TESTING TESTING 123 SERVER IP: " + IP);
            }

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
                            Player = new PlayerStats(Content.Load<Texture2D>("player"), Width, Height, 8, mapName);// it will generate a player with the default settings
                            sword = new Weapon(Content.Load<Texture2D>("Sword"), new Rectangle(0, 0, 59, 100), Player);
                            BackMap.CollisionTiles.Clear();
                            map.CollisionTiles.Clear();

                            BackMap.Generate(BackMap.Read("Back" + Player.mapName + ".txt"), 75);// this reads in the map and generates it
                            map.Generate(map.Read(Player.mapName + ".txt"), 75);// this reads in the map and generates it

                            client = new TcpClient();
                            client.NoDelay = true;
                            client.Connect(IP, PORT);
                            readBuffer = new byte[BUFFER_SIZE];
                            client.GetStream().BeginRead(readBuffer, 0, BUFFER_SIZE, StreamReceived, null);


                            System.Threading.Thread.Sleep(2000);

                            writeStream.Position = 0;
                            writer.Write((byte)Protocol.MapJoined);
                            writer.Write(Convert.ToInt32(Player.Position.X));
                            writer.Write(Convert.ToInt32(Player.Position.Y));
                            writer.Write(Player.mapName);
                            writer.Write(Player.name);
                            SendData(GetDataFromMemoryStream(writeStream));



                            System.Threading.Thread.Sleep(2000);



                            writeStream.Position = 0;
                            writer.Write((byte)Protocol.enemyLoad);
                            writer.Write(Player.mapName);
                            SendData(GetDataFromMemoryStream(writeStream));

                            CurrentGameState = GameState.Playing;//sets the gamestate to playing
                        }
                        else if (Typed != "Invalid")// if typed wasnt blank
                        {
                            Player = new PlayerStats(Typed, Content.Load<Texture2D>("player"), Width, Height, 8, out Typed);// makes a player using typed as the name
                            sword = new Weapon(Content.Load<Texture2D>("Sword"), new Rectangle(0, 0, 59, 100), Player);
                            BackMap.CollisionTiles.Clear();
                            map.CollisionTiles.Clear();

                            BackMap.Generate(BackMap.Read("Back" + Player.mapName + ".txt"), 75);// this reads in the map and generates it
                            map.Generate(map.Read(Player.mapName + ".txt"), 75);// this reads in the map and generates it


                            client = new TcpClient();
                            client.NoDelay = true;
                            client.Connect(IP, PORT);
                            readBuffer = new byte[BUFFER_SIZE];
                            client.GetStream().BeginRead(readBuffer, 0, BUFFER_SIZE, StreamReceived, null);

                            System.Threading.Thread.Sleep(2000);

                            writeStream.Position = 0;
                            writer.Write((byte)Protocol.MapJoined);
                            writer.Write(Convert.ToInt32(Player.Position.X));
                            writer.Write(Convert.ToInt32(Player.Position.Y));
                            writer.Write(Player.mapName);
                            writer.Write(Player.name);
                            SendData(GetDataFromMemoryStream(writeStream));



                            System.Threading.Thread.Sleep(2000);


                            writeStream.Position = 0;
                            writer.Write((byte)Protocol.enemyLoad);
                            writer.Write(Player.mapName);
                            SendData(GetDataFromMemoryStream(writeStream));

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
                        lock (client)
                        {
                            client.Close();
                        }
                        Mouse.SetPosition(0, 0);//moves the mouse
                        friendlyPlayers.Clear();
                        EnemyList.Clear();
                        weapons.Clear();
                        CurrentGameState = GameState.MainMenu;//goes to the main menu
                    }
                    //Friendlies a
                    lock (friendlyPlayers)
                    {
                        foreach (EntitySprite friendly in friendlyPlayers.Values)
                        {
                            friendly.Update(gameTime);
                        }
                    }
                    //Collison
                    foreach (CollisionTiles tile in map.CollisionTiles)// checks for every tile in the maps list of tiles
                    {
                        Player.CollisionTile(tile.Rectangle, map.Width, map.Height);//if the player is touching it
                        lock (EnemyList)
                        {
                            foreach (Enemy enemy in EnemyList.Values)// and for every slime in the Slimes list
                            {
                                enemy.CollisionTile(tile.Rectangle, map.Width, map.Height);// if the slime is touching the tiles
                            }
                        }
                        lock (friendlyPlayers)
                        {
                            foreach (EntitySprite friendly in friendlyPlayers.Values)
                            {
                                friendly.CollisionTile(tile.Rectangle, map.Width, map.Height);
                            }
                        }
                        lock (NPCs)
                        {
                            foreach (EntitySprite npc in NPCs.Values)
                            {
                                npc.CollisionTile(tile.Rectangle, map.Width, map.Height);
                            }
                        }

                    }
                    if (Player.mapName == "Town")
                    {
                        if (BackMap.CollisionTiles[21].Rectangle.Intersects(Player.POSRect) && Keyboard.GetState().IsKeyDown(Keys.Enter))
                        {
                            Player.Position = new Vector2(13641, 574);
                            Player.mapName = "World1";
                            CurrentGameState = GameState.MapSwitch;
                        }
                        
                    }
                    else if (Player.mapName == "World1")
                    {
                        if (BackMap.CollisionTiles[7988].Rectangle.Intersects(Player.POSRect) && Keyboard.GetState().IsKeyDown(Keys.Enter))
                        {
                            Player.Position = new Vector2(152, 799);
                            Player.mapName = "Town";
                            CurrentGameState = GameState.MapSwitch;
                        }
                    }

                    //Enemy
                    lock (EnemyList)
                    {
                        
                        foreach (var slime in EnemyList)//for every slime in the Slimes list
                        {
                            slime.Value.CollisionPlayer(Player, client);// check if its touching the player
                            slime.Value.Update(gameTime, map, Player, slime.Key, client);// update the slime
                            slime.Value.SlimeCollision(sword, Player, slime.Key,client);// and check if a sword is hitting it
                  
                            if (slime.Key == 20 && slime.Value.health <= 0 && Player.quest2Status == 1 )
                            {
                                Player.quest2Progress++;                            
                            }
                            else if (slime.Key < 10 && slime.Value.health <= 0 && Player.quest1Status == 1 )
                            {
                                Player.quest1Progress++;
                            }
                            else if (slime.Key > 10 && slime.Key < 20 && slime.Value.health <= 0 && Player.quest3Status == 1 )
                            {
                                Player.quest3Progress++;
                            }

                        }
                    }
                    foreach(var quest in quests)
                    {
                        
                        if (quest.Key == 21 && quest.Value.objectiveTarget >= Player.quest2Progress && Player.quest2Status == 1 )
                        {
                            Player.quest2Status = 2;
                            Player.quest2Progress = quest.Value.objectiveTarget;
                        }
                        if (quest.Key == 1 && quest.Value.objectiveTarget >= Player.quest1Progress && Player.quest1Status == 1 )
                        {
                            Player.quest1Status = 2;
                            Player.quest1Progress = quest.Value.objectiveTarget;

                        }
                        if (quest.Key == 2 && quest.Value.objectiveTarget >= Player.quest3Progress && Player.quest3Status == 1 )
                        {
                            Player.quest3Status = 2;
                            Player.quest3Progress = quest.Value.objectiveTarget;

                        }
                    }


                    //Player Sprite
                    Player.Update(gameTime, Player, client, writer,writeStream);//updates the player sprite
                    sword.update(Mouse.GetState(), GraphicsDevice.Viewport, sword.owner, writer, writeStream, client);// updates the sword


                    lock (NPCs)
                    {
                        foreach (EntitySprite npc in NPCs.Values)
                        {
                            npc.Update(gameTime);
                            
                        }
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

                    lock(NPCs)
                    {
                        foreach (var npc in NPCs)
                        {
                            npc.Value.CollisionPlayer(Player, Keyboard.GetState().IsKeyDown(Keys.Enter));
                            if (npc.Value.interacted)
                            {
                                if (npc.Key == 21)
                                {
                                    dmenu.Show(quests[npc.Key].getText(Player.quest2Status));

                                }
                                if (npc.Key == 1)
                                {
                                    dmenu.Show(quests[npc.Key].getText(Player.quest1Status));
                                }
                                if (npc.Key == 2)
                                {
                                    dmenu.Show(quests[npc.Key].getText(Player.quest3Status));
                                }
                            }
                        }

                    }

                   
                    dmenu.Update(Mouse.GetState(),Camera.centrer,Scrolling1.rectangle);
                    if (dmenu.ok.isclicked)
                    {
                        if (dmenu.text == quests[21].startText)
                        {
                            Player.quest2Status = 1;
                        }
                        if (dmenu.text == quests[21].completeText)
                        {
                            Player.experience = quests[21].xpReward;
                            Player.quest2Status = 3;
                        }

                        if (dmenu.text == quests[1].startText)
                        {
                            Player.quest1Status = 1;
                        }
                        if (dmenu.text == quests[1].completeText)
                        {
                            Player.experience = quests[1].xpReward;
                            Player.quest1Status = 3;
                        }

                        if (dmenu.text == quests[2].startText)
                        {
                            Player.quest3Status = 1;
                        }
                        if (dmenu.text == quests[2].completeText)
                        {
                            Player.experience = quests[2].xpReward;
                            Player.quest3Status = 3;
                        }

                    }
                    break;   //break to mark end of case
                case GameState.MapSwitch://gamestate MapSwitch
                    {
                        EnemyList.Clear();
                        friendlyPlayers.Clear();
                        NPCs.Clear();
                        map.CollisionTiles.Clear();
                        BackMap.CollisionTiles.Clear();
                        
                        BackMap.Generate(BackMap.Read("Back" + Player.mapName + ".txt"), 75);// this reads in the map and generates it
                        map.Generate(map.Read(Player.mapName + ".txt"), 75);// this reads in the map and generates it



                        writeStream.Position = 0;
                        writer.Write((byte)Protocol.MapJoined);
                        writer.Write(Convert.ToInt32(Player.Position.X));
                        writer.Write(Convert.ToInt32(Player.Position.Y));
                        writer.Write(Player.mapName);
                        writer.Write(Player.name);
                        SendData(GetDataFromMemoryStream(writeStream)); 



                        System.Threading.Thread.Sleep(2000);

                        writeStream.Position = 0;
                        writer.Write((byte)Protocol.enemyLoad);
                        writer.Write(Player.mapName);
                        SendData(GetDataFromMemoryStream(writeStream));

                        CurrentGameState = GameState.Playing;

                        break;
                    }
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
                    spriteBatch.DrawString(font, IP, new Vector2(GraphicsDevice.Viewport.Width - 10, GraphicsDevice.Viewport.Height - 10), Color.LimeGreen);
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
                    lock (EnemyList)
                    {
                        foreach (Enemy enemy in EnemyList.Values)//draws the slimes
                        {
                            enemy.Draw(spriteBatch, PlayerFont);
                        }
                    }
                    lock (NPCs)
                    {
                        foreach (EntitySprite npc in NPCs.Values)
                        {
                            npc.Draw(spriteBatch, PlayerFont);
                        }
                    }
                    //Player           
                    Player.Draw(spriteBatch);//draws the player
                    Player.DrawStatsDisplay(spriteBatch, PlayerFont, graphics.GraphicsDevice.Viewport, Keyboard.GetState(), menu1, menu2, menu3, Camera.centrer, Scrolling1.rectangle, hpBar, manaBar, barBack);//it then draws the stats menu in the top left
                    sword.Draw(spriteBatch, Player);// draws the sword

                    //Friendlies Sprites
                    lock (friendlyPlayers)
                    {
                        foreach (EntitySprite friendly in friendlyPlayers.Values)
                        {
                            friendly.Draw(spriteBatch, PlayerFont);
                        }
                    }
      
                    lock (weapons)
                    {
                        foreach (Weapon wep in weapons)
                        {
                            wep.Draw(spriteBatch, wep.owner);
                        }

                    }
                 
                    dmenu.Draw(spriteBatch, graphics.GraphicsDevice.Viewport, Camera.centrer,Scrolling1.rectangle);

                    int questsInProgress = 1;
                    if (Player.quest1Status == 1 || Player.quest1Status == 2)
                    {
                        questsInProgress++;
                        spriteBatch.DrawString(font, "A New Beginning\nSlimes " + Player.quest1Progress + "/" + quests[1].objectiveTarget, new Vector2(Camera.centrer.X + Scrolling1.rectangle.Width / 2 - 800 + 200, (Camera.centrer.Y - Scrolling1.rectangle.Height * 2 / 3 + 120 + 400 + 32*questsInProgress)),Color.DarkSalmon);
                    }
                    if (Player.quest2Status == 1 || Player.quest2Status == 2)
                    {
                        questsInProgress++;
                        spriteBatch.DrawString(font, "A Tough Challenge\nBoss Slime " + Player.quest2Progress + "/" + quests[21].objectiveTarget, new Vector2(Camera.centrer.X + Scrolling1.rectangle.Width / 2 - 800 + 200, (Camera.centrer.Y - Scrolling1.rectangle.Height * 2 / 3 + 120 + 400 + 32 * questsInProgress*questsInProgress)), Color.DarkSalmon);

                    }
                    if (Player.quest3Status == 1 || Player.quest3Status == 2)
                    {
                        questsInProgress++;
                        spriteBatch.DrawString(font, "Final Exam\nZombies " + Player.quest3Progress + "/" + quests[2].objectiveTarget, new Vector2(Camera.centrer.X + Scrolling1.rectangle.Width / 2 - 800 + 200, (Camera.centrer.Y - Scrolling1.rectangle.Height * 2 / 3 + 120 + 400 + 32 * questsInProgress*questsInProgress)), Color.DarkSalmon);
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

                }
                else if (p == Protocol.Disconnected)
                {
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();
                    if(friendlyPlayers.ContainsKey(id))
                    {
                        lock (friendlyPlayers)
                        {
                            friendlyPlayers.Remove(id);
                        }
                        
                    }
                }
                else if (p == Protocol.MapJoined)
                {
                 
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    string map = reader.ReadString();
                    string name = reader.ReadString();
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();

                    lock (friendlyPlayers)
                    {
                        if (map == Player.mapName && !friendlyPlayers.ContainsKey(id))
                        {

                            friendlyPlayers.Add(id, new EntitySprite(Content.Load<Texture2D>("player2"), new Vector2(x, y), Width, Height, 8, name));

                            writeStream.Position = 0;
                            writer.Write((byte)Protocol.MapJoined);
                            writer.Write(Convert.ToInt32(Player.Position.X));
                            writer.Write(Convert.ToInt32(Player.Position.Y));
                            writer.Write(Player.mapName);
                            writer.Write(Player.name);
                            SendData(GetDataFromMemoryStream(writeStream));
                        }
                        else if (map != Player.mapName && friendlyPlayers.ContainsKey(id))
                        {

                            friendlyPlayers.Remove(id);

                        }
                        else if (map == Player.mapName && friendlyPlayers.ContainsKey(id))
                        {
                            friendlyPlayers.Remove(id);
                            friendlyPlayers.Add(id, new EntitySprite(Content.Load<Texture2D>("player2"), new Vector2(x, y), Width, Height, 8, name));
                        }
                    }
                    
                }
                else if (p == Protocol.Load)
                {
       
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    string map = reader.ReadString();
                    string name = reader.ReadString();
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();

                    if (map == Player.mapName && !friendlyPlayers.ContainsKey(id))
                    {
                        lock (friendlyPlayers)
                        {
                            friendlyPlayers.Add(id, new EntitySprite(Content.Load<Texture2D>("player2"), new Vector2(x, y), Width, Height, 8, name));
                        }
                        writeStream.Position = 0;
                        writer.Write((byte)Protocol.MapJoined);
                        writer.Write(Convert.ToInt32(Player.Position.X));
                        writer.Write(Convert.ToInt32(Player.Position.Y));
                        writer.Write(Player.mapName);
                        writer.Write(Player.name);
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
                        friendlyPlayers[id].Position.X = posX;
                        friendlyPlayers[id].Position.Y = posY;  
                        friendlyPlayers[id].velocity.Y = y;
                        friendlyPlayers[id].velocity.X = x;
                        
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
                else if (p == Protocol.enemyActionChanged)
                {
                    int id = reader.ReadInt32();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    int act = reader.ReadInt32();
                    if (EnemyList.ContainsKey(id))
                    {
                        EnemyList[id].Position.X = x;
                        EnemyList[id].Position.Y = y;
                        EnemyList[id].action = act;                     
                    }
                }
                else if (p == Protocol.enemyLoad)//10
                {
                    string map = reader.ReadString();
                    int id = reader.ReadInt32();
                    string texture = reader.ReadString();
                    int health = reader.ReadInt32();
                    int strength = reader.ReadInt32();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    int width = reader.ReadInt32();
                    int height = reader.ReadInt32();
                    int frames = reader.ReadInt32();
                    int spawntimer = reader.ReadInt32();
                    bool isPassive = reader.ReadBoolean();
                    int act = reader.ReadInt32();

                    if (map == Player.mapName && !EnemyList.ContainsKey(id))
                    {
                        lock (EnemyList)
                        {
                            EnemyList.Add(id, new Enemy(Content.Load<Texture2D>(texture), health, strength, new Vector2(x, y), width, height, frames, spawntimer, isPassive));
                            EnemyList[id].action = act;
                        }
                    }
                }
                else if (p == Protocol.enemyDied)
                {
                    int id = reader.ReadInt32();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    int act = reader.ReadInt32();
                    int hp = reader.ReadInt32();
                    if (EnemyList.ContainsKey(id))
                    {
                        EnemyList[id].Position.X = x;
                        EnemyList[id].Position.Y = y;
                        EnemyList[id].action = act;
                        EnemyList[id].health = hp;
                        if (id < 10 && Player.quest1Status == 1)
                        {
                            Player.quest1Progress++;
                        }
                        else if (id < 20 && Player.quest3Status == 1)
                        {
                            Player.quest3Progress++;
                        }
                        else if (id == 20 && Player.quest2Status == 1)
                        {
                            Player.quest2Progress++;
                        }
                    }

                }
                else if (p == Protocol.enemyHit)
                {
                    int id = reader.ReadInt32();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    int act = reader.ReadInt32();
                    int hp = reader.ReadInt32();
                    
                    if (EnemyList.ContainsKey(id))
                    {
                        EnemyList[id].Position.X = x;
                        EnemyList[id].Position.Y = y;
                        EnemyList[id].action = act;
                        EnemyList[id].Health = hp;
                        EnemyList[id].hit = true;
                        EnemyList[id].hitTimer = 60;
                    }
                }
                else if (p == Protocol.playerHit)
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    int posX = reader.ReadInt32();
                    int posY = reader.ReadInt32();
                    byte id = reader.ReadByte();
                    if (friendlyPlayers.ContainsKey(id))
                    {
                        friendlyPlayers[id].hit = true;
                        friendlyPlayers[id].hitTimer = 60;
                        friendlyPlayers[id].Position = new Vector2(posX, posY);
                        friendlyPlayers[id].velocity = new Vector2(x, y);
                    }
                }
                else if (p == Protocol.enemySync)
                {
                    int mobID = reader.ReadInt32();
                    if (EnemyList.ContainsKey(mobID))
                    {
                        writeStream.Position = 0;
                        writer.Write((byte)Protocol.enemySync);
                        writer.Write(Player.mapName);
                        writer.Write(Convert.ToInt32(mobID));
                        writer.Write(EnemyList[mobID].Position.X);
                        writer.Write(EnemyList[mobID].Position.Y);
                        SendData(GetDataFromMemoryStream(writeStream));
                    }
                }
                else if (p == Protocol.npcLoad)//10
                {
                    string map = reader.ReadString();
                    int id = reader.ReadInt32();
                    string texture = reader.ReadString();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    int width = reader.ReadInt32();
                    int height = reader.ReadInt32();


                    if (map == Player.mapName && !NPCs.ContainsKey(id))
                    {
                        lock (NPCs)
                        {
                            NPCs.Add(id, new EntitySprite(Content.Load<Texture2D>(texture), new Vector2(x, y), width, height, 1, texture));
                            

                        }
                    }
                }




            }
            catch (Exception e)
            {
            /*  lock (friendlyPlayers)
                {
                    friendlyPlayers.Clear();
                }
                writeStream.Position = 0;
                writer.Write((byte)Protocol.MapJoined);
                writer.Write(Convert.ToInt32(Player.Position.X));
                writer.Write(Convert.ToInt32(Player.Position.Y));
                writer.Write(Player.mapName);
                writer.Write(Player.name);
                SendData(GetDataFromMemoryStream(writeStream));
                */
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
