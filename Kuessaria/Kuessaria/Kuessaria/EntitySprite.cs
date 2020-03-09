using Microsoft.Xna.Framework;
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
    class EntitySprite : Sprite//this player sprite holds animated sprites that can move
    {
        public Rectangle POSRect;//this is the position rectangle the character is at
        public bool hit = false;// this is a boolean determining if object was hit
        public int Width;//this is the width of 1 frame of the object
         public int Height;//this is the height of 1 frame of the object
         public int currentFrame;//this is the value to hold which frame is being drawn
         protected Vector2 origin;//this is the center position of the object
         public Vector2 velocity;// this holds the velocity of the object in the x and y directions
         protected bool letgo;//this is for the player specifically, for when they hold down the jump key
         public bool Jumped = false;//this is a boolean to determine if they jumped
         protected float animtimer;//this is a float that holds a value for how long the frame has been on for
         protected int interval = 50;// tthis is an interval that it will wait to goto the next frame
        public float hitTimer = 0;// this is a timer that determines if an object is ready to get hit again
        
        public int Frames;//this is the total number of frames
        public EntitySprite()
        {

        }

        public EntitySprite(Texture2D newTexture, Vector2 newPosition, int newWidth, int newHeight, int frames)// this constructor creates the object using the values it takes in
        {
            Frames = frames;
            texture = newTexture;
            Width = newWidth;
            Height = newHeight;
            Position = newPosition;
            rectangle = new Rectangle((int)newPosition.X, (int)newPosition.Y, Width, Height);
            
        }
        public virtual void Update(GameTime gameTime, PlayerStats stats, TcpClient client, BinaryWriter writer, MemoryStream writeStream)//this is the update method that runs constantly
        {

            Move(gameTime, stats, client, writer, writeStream);//this checks if the player is moving
            rectangle = new Rectangle(currentFrame * Width, 0, Width, Height);//this rectangle is the one that determines what is being drawn
            origin = new Vector2(rectangle.Width / 2, rectangle.Height / 2);//this is the center of that rectangle
            Position += velocity;//this adds velocity to the position to move the object
            POSRect = new Rectangle((int)Position.X - Width/2, (int)Position.Y - Height/2, Width, Height);// this is the hitbox of the character, a rectangle at their position

            if (hitTimer <= 0)//if the hit timer is less than or equal to 0
            {
                hit = false;//it will set hit back to false
                hitTimer = 0;// and keep the hittimer at 0
            }
            else//if it wasnt 0
            {
                hitTimer--;//it will attempt to bring it down to zero
            }


        }
        internal void Update(GameTime gameTime)
        {
            MoveIndependent(gameTime);
            rectangle = new Rectangle(currentFrame * Width, 0, Width, Height);//this rectangle is the one that determines what is being drawn
            origin = new Vector2(rectangle.Width / 2, rectangle.Height / 2);//this is the center of that rectangle
            Position += velocity;//this adds velocity to the position to move the object
            POSRect = new Rectangle((int)Position.X - Width / 2, (int)Position.Y - Height / 2, Width, Height);// this is the hitbox of the character, a rectangle at their position



            if (hitTimer <= 0)//if the hit timer is less than or equal to 0
            {
                hit = false;//it will set hit back to false
                hitTimer = 0;// and keep the hittimer at 0
            }
            else//if it wasnt 0
            {
                hitTimer--;//it will attempt to bring it down to zero
            }
        }

        private void MoveIndependent(GameTime gameTime)
        {
            bool right = false;
            bool stopped = false;
            if (velocity.X > 0)
            {
                right = true;
                AnimateRight(gameTime);
            }
            else if (velocity.X < 0)
            {               
                AnimateLeft(gameTime);
            }
            else
            {
                stopped = true;
            }
            velocity.X = (float)Math.Round((velocity.X * 0.85), 2);//it will round their left right velocity to slowly slow them down
            if (!stopped && Math.Abs(velocity.X) < 0.05)
            {
                velocity.X = 0;
                if (right)
                {
                    currentFrame = 3;
                }
                else
                {
                    currentFrame = 4;
                }
            }


            if (velocity.Y < 20)//this is the constant of gravity that is added if they are not at terminal velocity
                velocity.Y += 0.4f;

            if (velocity.Y >= 20)//this ensures they dont go past the terminal velocity of 20
            {
                velocity.Y = 20;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, rectangle, Color.White, 0f, origin, 1.0f, SpriteEffects.None, 0);// this simply draws the sprite
            
        }
        //Movement Methods
        public virtual void Jump()//this is for jumping
        {
            
            velocity.Y = -16f;//this makes the player move upward
            letgo = false;//this is sort of a pastkey type deal
            Jumped = true;//this makes sure the player cant jump again yet

        }
        public virtual void Move(GameTime gameTime, PlayerStats Stats, TcpClient client, BinaryWriter writer, MemoryStream writeStream)//this determines the players movement

        {
            if (Keyboard.GetState().IsKeyDown(Keys.D))//if they are pressing d
            {
                AnimateRight(gameTime);//it animates it to the right
                velocity.X = Stats.mSpeed;//and sets their MSpeed to their velocity
                writeStream.Position = 0;
                writer.Write((byte)Protocol.PlayerMoved);
                writer.Write(this.velocity.X);
                writer.Write(this.velocity.Y);
                SendData(GetDataFromMemoryStream(writeStream), client);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.A))// this does the same but for left
            {
                AnimateLeft(gameTime);
                velocity.X = -Stats.mSpeed;
                writeStream.Position = 0;
                writer.Write((byte)Protocol.PlayerMoved);
                writer.Write(this.velocity.X);
                writer.Write(this.velocity.Y);
                SendData(GetDataFromMemoryStream(writeStream), client);
            }
            else// if the player isnt pressing left or right it will do all this
            {
                velocity.X = (float)Math.Round((velocity.X * 0.85),2);//it will round their left right velocity to slowly slow them down

                if (velocity.X > 0)//if their velocity is greater than zero when they stopped
                {
                    currentFrame = 3;//it sets them in the right idle frame
                    if (velocity.X < 0.05)// if its less than 0.05 it rounds it down to 0 so that the player doesnt  move indefinitely
                    {
                        velocity.X = 0f;
                    }
                }
                if (velocity.X < 0)// does the same as the  above but for moving left
                {
                    currentFrame = 4;
                    if (velocity.X > -0.05)
                    {
                        velocity.X = 0f;
                    }
                }
                
            }
            if (Jumped == true && (Keyboard.GetState().IsKeyDown(Keys.W)))// if they press W and jumped is true this is added to help slow the player down
            {
                float i = 1;
                velocity.Y += 0.15f * i;

            }
            else if (Jumped == true && Keyboard.GetState().IsKeyUp(Keys.W) && letgo == false && velocity.Y < 0)//is they jumped and have put the w key up and they havent letgo already and the velocity isnt less than 0 already
            {
                velocity.Y += 5;//it adds 5 to the velocity, to slow them down because they let go of w
                letgo = true;//sets letgo to true so this doesnt run again
            }
            if (velocity.Y >= 20)//this ensures they dont go past the terminal velocity of 20
            {
                velocity.Y = 20;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W) && Jumped == false)//if they press w and havent jumped yet
            {
                Jump();//theyll jump
                writeStream.Position = 0;
                writer.Write((byte)Protocol.PlayerMoved);
                writer.Write(this.velocity.X);
                writer.Write(this.velocity.Y);
                SendData(GetDataFromMemoryStream(writeStream),client);
            }

            if (velocity.Y < 20)//this is the constant of gravity that is added if they are not at terminal velocity
                velocity.Y += 0.4f;
        
        }

        public virtual void Move(GameTime gameTime, Vector2 speed)//this determines the players movement

        {
            if (speed.X > 0)//if they are pressing d
            {
                AnimateRight(gameTime);//it animates it to the right
                velocity.X = speed.X;//and sets their MSpeed to their velocity
            }
            else if (speed.X < 0)// this does the same but for left
            {
                AnimateLeft(gameTime);
                velocity.X = speed.X;
            }
            else// if the player isnt pressing left or right it will do all this
            {
                velocity.X = (float)Math.Round((velocity.X * 0.85), 2);//it will round their left right velocity to slowly slow them down

                if (velocity.X > 0)//if their velocity is greater than zero when they stopped
                {
                    currentFrame = 3;//it sets them in the right idle frame
                    if (velocity.X < 0.05)// if its less than 0.05 it rounds it down to 0 so that the player doesnt  move indefinitely
                    {
                        velocity.X = 0f;
                    }
                }
                if (velocity.X < 0)// does the same as the  above but for moving left
                {
                    currentFrame = 4;
                    if (velocity.X > -0.05)
                    {
                        velocity.X = 0f;
                    }
                }

            }
            if (Jumped == true && (Keyboard.GetState().IsKeyDown(Keys.W)))// if they press W and jumped is true this is added to help slow the player down
            {
                float i = 1;
                velocity.Y += 0.15f * i;

            }
            else if (Jumped == true && Keyboard.GetState().IsKeyUp(Keys.W) && letgo == false && velocity.Y < 0)//is they jumped and have put the w key up and they havent letgo already and the velocity isnt less than 0 already
            {
                velocity.Y += 5;//it adds 5 to the velocity, to slow them down because they let go of w
                letgo = true;//sets letgo to true so this doesnt run again
            }
            if (velocity.Y >= 20)//this ensures they dont go past the terminal velocity of 20
            {
                velocity.Y = 20;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W) && Jumped == false)//if they press w and havent jumped yet
                Jump();//theyll jump

            if (velocity.Y < 20)//this is the constant of gravity that is added if they are not at terminal velocity
                velocity.Y += 0.4f;

        }

        //Animation Methods
        public virtual void AnimateRight(GameTime gameTime)
        {
            animtimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 2;// this counts the milliseconds
            if (animtimer > interval)//and if the milliseconds are greater then the interval set to wait to switch frames
            {
                currentFrame++;// it adds one to the frame
                animtimer = 0;// and resets the milliseconds
                if (currentFrame > Frames/2-1)// if the frame is greater than the last frame facing right
                    currentFrame = 0;// it brings the frame back to 0
            }

        }
        public virtual void AnimateLeft(GameTime gameTime)// this is the same as the above but to the left
        {
            animtimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 2;
            if (animtimer > interval)
            {
                currentFrame++;
                animtimer = 0;
                if (currentFrame > Frames - 1 || currentFrame < Frames/2)
                    currentFrame = Frames/2;
            }

        }
        //Collision Method
        public virtual void CollisionTile(Rectangle newRectangle, int xOffset, int yOffset)//this checks if the player is colliding with the blocks
        {
            if (POSRect.TouchBottom(newRectangle) && POSRect.TouchTop(POSRect))// if the block and character hitbox are touching at the bottom and top
            {
                Position.Y = newRectangle.Y + Height * 2;//it makes the character goto the bottom of the block and fall downwards, this is so if somehow the character gets in a block its not stuck
                velocity.Y = 1f;
            }
            if (POSRect.TouchTop(newRectangle))//if its touching the top of the tile
            {
                
                Position.Y = newRectangle.Y - Height/2 -1;// it puts the player right on top of the tile
                velocity.Y = 0f;//and kills their velocity
                Jumped = false;//and sets jumped to false so they can jump again
                
            }
            if (POSRect.TouchLeft(newRectangle))//if its touching the left
            {
                Position.X = newRectangle.X - Width/2 - 2;// it keeps setting the position to the left of the block 
            }
            if (POSRect.TouchRight(newRectangle))//if its touching the right
            {
                Position.X = newRectangle.X + Width/2 + newRectangle.Width + 2;// it sets the characters position to the right of the block
            }
            if (POSRect.TouchBottom(newRectangle) && velocity.Y < 1f)//if its touching the bottom and youre not already headed downwards, it sends you downwards
            {
                velocity.Y = 1f;
            }
            
            if (Position.X < 0) Position.X = 0;//the tile map starts at 0 so if the position of the character is less than that it sends them back to the edge
            if (Position.X > xOffset - Width/2) Position.X = xOffset - Width/2;// if their position is greater than the whole map - their width it keeps them inside the map
            if (Position.Y < 0) velocity.Y = 1f;//if they try and jump out, it sends them back down
            if (Position.Y > yOffset - Height/2) Position.Y = yOffset - Height/2;//ifthey fall out at the bottom of the map it holds them in
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
