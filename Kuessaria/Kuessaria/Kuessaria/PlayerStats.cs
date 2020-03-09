using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuessaria
{
    class PlayerStats : EntitySprite
    {
        /// <summary>
        /// All these stats are self explanatory but the 
        /// encapsulation is there so that they are publicaly
        /// accesible and so that you can add values to them
        /// the health and mana have a maximum and thus their 
        /// values can not go past that
        /// </summary>
        int level;
        public int Level
        {
            get { return level; }
            set { level += value; }
        }
        private int Health;
        public int health
        {
            get { return Health; }
            set
            {
                Health += value;
                if (Health > MaxHealth)
                {
                    Health = MaxHealth;
                }
            }
        }
        private int Mana;
        public int mana
        {
            get { return Mana; }
            set
            {
                Mana += value;
                if (Mana > MaxMana)
                {
                    Mana = MaxMana;
                }
            }
        }
        private string Name;
        public string name
        {
            get { return Name; }
            set { Name = value; }
        }
        private int Strength;
        public int strength
        {
            get { return Strength; }
            set { Strength += value; }
        }
        private int MSpeed;
        public int mSpeed
        {
            get { return MSpeed; }
            set { MSpeed += value; }
        }
        private int ASpeed;
        public int aSpeed
        {
            get { return ASpeed; }
            set { ASpeed += value; }
        }
        private int Intelligence;
        public int intelligence
        {
            get { return Intelligence; }
            set { Intelligence += value; }
        }
        private int Experience;
        public int experience
        {
            get { return Experience; }
            set { Experience += value; }
        }
        private int MaxHealth;
        public int maxHealth
        {
            get { return MaxHealth; }
            set { MaxHealth += value; }
        }
        private int MaxMana;


        public int maxMana
        {
            get { return MaxMana; }
            set { MaxMana += value; }
        }
        enum QuickState//this is the state of the menu in the top left
        {
            MenuOn,
            MenuOff,
            LevelUp,
        }
        QuickState CurrentQuickState = QuickState.MenuOff;// this is the curresnt state used in the switch
        public PlayerStats()
        {

        }
        public PlayerStats(string Filename, Texture2D newTexture, int newWidth, int newHeight, int frames, out string Typed)//this constructor is used if they insert a name
        {
            Frames = frames;//sets the amount of frames the character has
            string[] stats;// prepares an array to hold the stats 
            try//it trys to get the states from a file
            {
                stats = System.IO.File.ReadAllLines("Saves/" + Filename + ".txt");
            }
            catch//but if they werent their it makes a new one with all the default values
            {
                Health = 100;
                MaxHealth = 100;
                Mana = 100;
                MaxMana = 100;
                Name = Filename;// and the name they selected
                Strength = 10;
                MSpeed = 5;
                ASpeed = 100;
                Intelligence = 10;
                Experience = 0;
                level = 1;


                texture = newTexture;
                Width = newWidth;
                Height = newHeight;
                Position = new Vector2(250, 0);
                rectangle = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
                try//it then trys to save the character
                {
                    if (Filename != "Invalid")//if the character name isnt invalid
                        SaveCharacter();//it saves
                    Typed = Filename;// and sets typed to the filename
                }
                catch//if it couldnt save
                {
                    Typed = "Invalid";//it sets typed to invalid so they can retry
                }
                return;//it then returns so the next part doesnt run
            }
            Name = stats[0];// these lines set all the values to the character to whatever was in hte file
            Health = int.Parse(stats[1]);
            MaxHealth = int.Parse(stats[2]);
            Mana = int.Parse(stats[3]);
            MaxMana = int.Parse(stats[4]);
            Strength = int.Parse(stats[5]);
            MSpeed = int.Parse(stats[6]);
            ASpeed = int.Parse(stats[7]);
            Intelligence = int.Parse(stats[8]);
            Experience = int.Parse(stats[9]);
            level = int.Parse(stats[10]);

            texture = newTexture;
            Width = newWidth;
            Height = newHeight;
            Position = new Vector2(int.Parse(stats[11]), int.Parse(stats[12]));
            rectangle = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            Typed = Filename;
            
        }


        public PlayerStats(Texture2D texture2D, int width, int height, int frames)// this creates the character when they didnt input a value
        {
            Health = 100;// it sets all the values to their defaults
            maxHealth = 100;
            Mana = 100;
            MaxMana = 100;
            string[] RandomName = System.IO.File.ReadAllLines("DefaultNames.txt");//and selects a random name from the text file of random names
            Random RNG = new Random();
            Name = RandomName[RNG.Next(0, RandomName.Length)];
            Strength = 10;
            MSpeed = 5;
            ASpeed = 100;
            Intelligence = 10;
            level = 1;
            Experience = 0;
            Frames = frames;



            texture = texture2D;
            Width = width;
            Height = height;
            Position = new Vector2(250, 0);
            rectangle = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

            
        }

        public void SaveCharacter()// the save character function is how all the stats are saved to the file
        {
            string[] stats = new string[13];
            stats[0] = Name;
            stats[1] = Health.ToString();
            stats[2] = MaxHealth.ToString();
            stats[3] = Mana.ToString();
            stats[4] = MaxMana.ToString();
            stats[5] = Strength.ToString();
            stats[6] = MSpeed.ToString();
            stats[7] = ASpeed.ToString();
            stats[8] = Intelligence.ToString();
            stats[9] = Experience.ToString();
            stats[10] = level.ToString(); 

            stats[11] = POSRect.X.ToString();
            stats[12] = POSRect.Y.ToString();
            // it sets the string array to all the stats

            System.IO.File.WriteAllLines("Saves/" + Name + ".txt", stats);// then saves it as a .txt in the saves folder

        }
        public void Draw(SpriteBatch spriteBatch)//this draw takes a few things into account
        {
            if (hit)// if the player was hit
            {
                spriteBatch.Draw(texture, Position, rectangle, Color.DarkRed, 0f, origin, 1.0f, SpriteEffects.None, 0);//it draws them exactly the same but Color.Orange
            }
            else//if they werent hit
            {
                spriteBatch.Draw(texture, Position, rectangle, Color.White, 0f, origin, 1.0f, SpriteEffects.None, 0);//it draws them normally
            }
            

        }
        public void LevelUp()//the level up method sets the quickstate menu state to level up
        {
            CurrentQuickState = QuickState.LevelUp;
        }
        public void LevelUpdate(KeyboardState keyboard)//this checks whatever the character presses to see what to level up
        {
            if (keyboard.IsKeyDown(Keys.D1))// they press 1 and it levels up the health
            {
                MaxHealth += 10;
                Health += 10;
                Experience -= level * 100;//takes away the amount of experience needed for that level
                level += 1;//adds one to their level
                
                CurrentQuickState = QuickState.MenuOff;//turns off the menu
            }
            if (keyboard.IsKeyDown(Keys.D2))//does the same but for mana
            {
                MaxMana += 10;
                Mana += 10;
                Experience -= level * 100;
                level += 1;
               
                CurrentQuickState = QuickState.MenuOff;
            }
            if (keyboard.IsKeyDown(Keys.D3))// the same but for strength
            {
                Strength += 5;
                Experience -= level * 100;
                level += 1;
                
                CurrentQuickState = QuickState.MenuOff;
            }
            if (keyboard.IsKeyDown(Keys.D4) && MSpeed <= 15)// same but for movement speed
            {
                MSpeed += 2;
                Experience -= level * 100;
                level += 1;
              
                CurrentQuickState = QuickState.MenuOff;
            }
            if (keyboard.IsKeyDown(Keys.D5))//same 
            {
                ASpeed += 5;
                Experience -= level * 100;
                level += 1;
                
                CurrentQuickState = QuickState.MenuOff;
            }
            if (keyboard.IsKeyDown(Keys.D6))// same
            {
                Intelligence += 5;
                Experience -= level * 100;
                level += 1;
                
                CurrentQuickState = QuickState.MenuOff;
            }
        }

        public void DrawStatsDisplay(SpriteBatch spriteBatch, SpriteFont font, Viewport Viewport, KeyboardState keyboard, Texture2D Menu1, Texture2D Menu2, Texture2D Menu3, Vector2 centre, Rectangle rectangle)// this draws the stat board
        {
            switch (CurrentQuickState)//this switch determines whether to draw all the stats or just health and mana
            {
                case QuickState.MenuOff://if the menu is off
                                        //it just draws the health and mana in the top left of the viewport
                    spriteBatch.Draw(Menu1, new Vector2(centre.X - rectangle.Width / 2 + 160, centre.Y - rectangle.Height * 2 / 3 + 120), Color.White);

                    spriteBatch.DrawString(font, Name, new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120)), Color.Orange);
                    spriteBatch.DrawString(font,"Health: "+ Health + "/" + MaxHealth, new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15), Color.Orange);
                    spriteBatch.DrawString(font,"Mana: " + Mana + "/" + MaxMana, new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120)+ 15*2), Color.Orange);
                    if (keyboard.IsKeyDown(Keys.LeftShift))// if they press shift
                        CurrentQuickState = QuickState.MenuOn;// it switches it into the menu on state
                    break;
                case QuickState.MenuOn://if the menus on
                    spriteBatch.Draw(Menu2, new Vector2(centre.X - rectangle.Width / 2 + 160, centre.Y - rectangle.Height * 2 / 3 + 120), Color.White);

                    spriteBatch.DrawString(font, Name, new Vector2(centre.X - rectangle.Width / 2 + 160, centre.Y - rectangle.Height * 2 / 3 + 120), Color.Orange);
                    spriteBatch.DrawString(font, "Health: " + Health + "/" + MaxHealth, new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120)+15), Color.Orange);
                    spriteBatch.DrawString(font, "Mana: " + Mana + "/" + MaxMana, new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15*2), Color.Orange);
                    spriteBatch.DrawString(font,"Strength: "+ Strength.ToString(), new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 3), Color.Orange);
                    spriteBatch.DrawString(font, "MSpeed: " + MSpeed.ToString(), new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 4), Color.Orange);
                    spriteBatch.DrawString(font, "ASpeed: " + ASpeed.ToString(), new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 5), Color.Orange);
                    spriteBatch.DrawString(font, "Intelligence: " + Intelligence.ToString(), new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 6), Color.Orange);
                    spriteBatch.DrawString(font, "Experience: " + Experience.ToString() + "/" + level * 100, new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 7), Color.Orange);


                    //it draws all the stats in the top left of the viewport
                    if (keyboard.IsKeyUp(Keys.LeftShift))//if the shift key is let go
                        CurrentQuickState = QuickState.MenuOff;//it shuts the menu off

                    break;
                case QuickState.LevelUp:// when they player is leveling up
                                        //it shows all the stats and which key to press to level them up
                    spriteBatch.Draw(Menu3, new Vector2(centre.X - rectangle.Width / 2 + 160, centre.Y - rectangle.Height * 2 / 3 + 120), Color.White);

                    spriteBatch.DrawString(font, Name, new Vector2(centre.X - rectangle.Width / 2 + 160, centre.Y - rectangle.Height * 2 / 3 + 120), Color.Orange);
                    spriteBatch.DrawString(font, "Health: " + Health + "/" + MaxHealth + " Press 1 to level up Health", new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15), Color.Orange);
                    spriteBatch.DrawString(font, "Mana: " + Mana + "/" + MaxMana + " Press 2 to level up Mana", new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 2), Color.Orange);
                    spriteBatch.DrawString(font, "Strength: " + Strength.ToString() + " Press 3 to level up Strength", new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 3), Color.Orange);
                    if (MSpeed <= 15)
                    {
                        spriteBatch.DrawString(font, "MSpeed: " + MSpeed.ToString() + " Press 4 to level up Move Speed", new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 4), Color.Orange);

                    }
                    else
                    {
                        spriteBatch.DrawString(font, "MSpeed: " + MSpeed.ToString() + " MAX", new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 4), Color.Orange);

                    }
                    spriteBatch.DrawString(font, "ASpeed: " + ASpeed.ToString() + " Press 5 to level up Attack Speed", new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 5), Color.Orange);
                    spriteBatch.DrawString(font, "Intelligence: " + Intelligence.ToString() + " Press 6 to level up Intelligence", new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 6), Color.Orange);
                    spriteBatch.DrawString(font, "Experience: " + Experience.ToString() +"/" + level*100, new Vector2(centre.X - rectangle.Width / 2 + 160, (centre.Y - rectangle.Height * 2 / 3 + 120) + 15 * 7), Color.Orange);

                    LevelUpdate(keyboard);//this is the level update method to see which key they press and do something accordingly
                    break;
            }

        }

    }
}
