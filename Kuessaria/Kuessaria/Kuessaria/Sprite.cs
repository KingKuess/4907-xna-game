﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Kuessaria
{
    class Sprite
    {
        public Texture2D texture;//this is the texture2d of the sprite, it is the image file the sprite looks like
        public Rectangle rectangle;//this is the rectangle that represents the position of the sprite and its width and height
        public Vector2 Position;// this is the literal position of the sprite
        
        public Sprite(Texture2D newTexture, Rectangle newRectangle)//this makes a new sprite using just a texture and rectangle
        {
            Position = new Vector2(newRectangle.X, newRectangle.Y);
            texture = newTexture;
            rectangle = newRectangle;
        }
        public Sprite()
        {
            
        }
        public virtual void Update()
        {

        }
        public virtual void Draw(SpriteBatch spriteBatch)//this just draws it
        {
            spriteBatch.Draw(texture, rectangle, Color.White);
        }
    }

    class Background : Sprite//this is the background sprite class,
    {
        public Background(Texture2D newTexture, Rectangle newRectangle)// it just makes a texture and a rectangle, it doesnt need anything else
        {
            texture = newTexture;
            rectangle = newRectangle;
        }
        public void Update(Vector2 centre, int width, int height)//and in the update method
        {
            float left = centre.X - rectangle.Width / 2;
            float top = centre.Y - rectangle.Height * 2 / 3;
            float right = centre.X + rectangle.Width / 2;
            float bottom = centre.Y + rectangle.Height / 3;

            if (left <= 0)
                rectangle.X = 0;
            else if (right >= width)
                rectangle.X = width - rectangle.Width;
            else
                rectangle.X = (int)left;// it just follows the characters x

            if (top <= 0)
                rectangle.Y = 0;
            else if (bottom >= height)
                rectangle.Y = height - rectangle.Height;
            else
                rectangle.Y = (int)top;// and y





        }

    }
    
    class Weapon : Sprite//this is the sword sprite, it has a ton of stuff going on inside
    {
        Vector2 spriteOrigin;//this is the center of the sprite
        public bool heal = false;//this is the bool to see if they are healing
        public int cooldown;// this is the cooldown on the heal
        public EntitySprite owner { get; set; }//the wielder of the weapon
        
        public enum Swinging//these are enums used for the swinging switch
        {
            Is,
            Not,
        }
        public Swinging swinging = Swinging.Not;//this is the Swinging used in the swinging switch
        float timer;// this is a timer used for swing time
        int interval = 125;// this is the interval the swing may last
        float rotation;// this holds the rotation value of the sword
        public Rectangle POSRect;//this holds the position of the sword
        public enum swung//this is the enum that is used to check if its swung left or right
        {
            left,
            right,
        }
        public swung SWUNG;//this is the swung that SWUNG
        public bool swinged;//this is a bool to check if they swung named swinged
        public Weapon(Texture2D newTexture, Rectangle newRectangle, EntitySprite Owner)////this creates the sword using just a texture and rectangle
        {
            owner = Owner;
            Position = new Vector2(newRectangle.X, newRectangle.Y);//it sets the position using the rectangles position

            texture = newTexture;
            rectangle = newRectangle;
            spriteOrigin = new Vector2(rectangle.Width / 2, rectangle.Height);// and the origin is the center of the width of thw sword but the bottom of the height, that wat it rotats from the handle, not the center

        }
        public void update()//this is the update method
        {
            if (cooldown > 0)// the the cooldown is greater than 0
            {
                cooldown--;//it reduces the cooldown


            }
            switch (swinging)// the switch to see if their swinging
            {
                case Swinging.Not://if their not
                    {
                        rotation = 0;// it removes the rotation from the sword

                        break;
                    }
                case Swinging.Is://if they are swinging
                    {

                        swing();//it swings
                        timer++;// and adds to the time
                        if (timer > interval - 100 / .98)// if the timer has been running for longer than the interval - aspeed
                        {
                            timer = 0;//it sets the timer to 0
                            swinging = Swinging.Not;//and it sets swinging to not
                            swinged = false;//and swinged to false to reset all the stuff

                        }
                        break;
                    }

            }
        }
        public void update(MouseState mouse, Viewport view, EntitySprite player, BinaryWriter writer, MemoryStream writeStream, TcpClient client)//this is the update method
        {
            if (mouse.X < view.Width /2 && swinged == false)//if the mouse if on the left side of the screen it sets swung to left
            {
                SWUNG = swung.left;


            }
            else if ( mouse.X >= view.Width/2 && swinged == false)//if its on the right and they havent swung it sets it to right
            {
                SWUNG = swung.right;
      
            }
            if (mouse.RightButton == ButtonState.Pressed && ((PlayerStats)player).mana >= 20 && cooldown <= 0)//if they rightclick and it isnt on cooldown and they have enough mana
            {
                ((PlayerStats)player).mana = -20;//it takes the mana
                ((PlayerStats)player).health = ((PlayerStats)player).intelligence;//and heals them
                cooldown = 100;//and puts the spell on CD
                heal = true;//and makes the heal bool true
                writeStream.Position = 0;
                writer.Write((byte)Protocol.weaponCreated);
                writer.Write((byte)2);
                SendData(GetDataFromMemoryStream(writeStream), client);
            }
            else if (cooldown > 0)// the the cooldown is greater than 0
            {
                cooldown--;//it reduces the cooldown
                
                
            }
            switch (swinging)// the switch to see if their swinging
            {
                case Swinging.Not://if their not
                    {
                        rotation = 0;// it removes the rotation from the sword
                        
                        if (mouse.LeftButton == ButtonState.Pressed)// when they left click
                        {
                            swinging = Swinging.Is;// it changes state to swinging
                        }
                        break;
                    }
                case Swinging.Is://if they are swinging
                    {
                        if (timer == 0)
                        {
                            writeStream.Position = 0;
                            writer.Write((byte)Protocol.weaponCreated);
                            if (mouse.X < view.Width / 2)
                            {
                                writer.Write((byte)0);
                            }
                            else
                            {
                                writer.Write((byte)1);
                            }
                            SendData(GetDataFromMemoryStream(writeStream), client);
                        }
                        swing();//it swings
                        timer++;// and adds to the time
                        if (timer > interval - ((PlayerStats)player).aSpeed/.98)// if the timer has been running for longer than the interval - aspeed
                        {
                            timer = 0;//it sets the timer to 0
                            swinging = Swinging.Not;//and it sets swinging to not
                            swinged = false;//and swinged to false to reset all the stuff

                        }
                        break;
                    }
                    
            }
        }
        void swing()
        {
            
            switch (SWUNG)//this switch for which way they swung determines the position of the sword
            {
                case swung.left://if they swung left
                    {
                        POSRect = new Rectangle((int)owner.POSRect.X - rectangle.Width * 2, (int)owner.POSRect.Y - rectangle.Height*1 / 4, rectangle.Width * 2, rectangle.Height*3/2);
                        //it puts the hitbox rectangle on the left side
                        Position.X = owner.POSRect.X + 5;//and the sprite position on the right of the player
                            Position.Y = owner.Position.Y;// and the y = to that of the player
                            rotation -= (float)100/1000;// and begins rotating based on attackspeed
                        swinged = true;//it also sets swinged to true




                        break;
                    }
                case swung.right:// if they swung right its the same but for the right side
                    {
                        POSRect = new Rectangle((int)owner.POSRect.X + rectangle.Width/2 + owner.POSRect.Width/2, (int)owner.POSRect.Y - rectangle.Height* 1 / 4, rectangle.Width * 2, rectangle.Height*3/2);

                        Position.X = owner.POSRect.X + owner.Width - 5;
                            Position.Y = owner.Position.Y;
                            rotation += (float)100 / 1000;
                        swinged = true;
                    }
                    break;

            }
        }

        public void Draw(SpriteBatch spriteBatch, EntitySprite player)
        {

            if (heal)// if they were healing it
            {
                spriteBatch.Draw(texture, new Vector2(player.POSRect.X, player.POSRect.Y - rectangle.Height), Color.Pink);//draws the pink sword of healing
                
                if (cooldown <= 0)// if the cooldown is < 0
                {
                    heal = false;// sets heal to false
                }
            }
            switch (swinging)// this is the switch to determine if they are swinging or not
            {
                case Swinging.Not://if their not it doesnt draw
                    {
                        break;
                    }
                case Swinging.Is://if it is it draws the sword
                    {
                        
                        spriteBatch.Draw(texture, Position, null, Color.White, rotation, spriteOrigin, 1f, SpriteEffects.None, 0);
                        break;
                    }
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
        public void SendData(byte[] b, TcpClient client)
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

