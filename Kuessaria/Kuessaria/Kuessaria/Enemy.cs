using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuessaria
{
    class Enemy : PlayerSprite
    {
        // The next values use encapsulation so they can be publicly obtained and set, they are all labelled self explainingly and thus only special ones will be commented
        int Health;
        int MaxHealth;
        public int health
        {
            get { return Health; }
            set { Health += value; }
        }
        int Strength;
        float movetimer;// this float will hold the amount of time since the last movement action was set
        int moveinterval = 500;//this is how long of an interval it will wait before moving
        int action;// this is a random number to decide which action will happen
        int spawntimer;// this is the timer that will determine if the slime exists yet or not
        int exist = 0;//it will not exist to begin with and thus exist will start at zero and wait until it is at spawntimer to exist
        Random rng = new Random();//a random number generated for rng purposes
        enum Spawned//this enum will determine if it is spawned or not
        {
            Not,
            Is,
        }
        Spawned currentSpawned = Spawned.Not;// sets the default of the currentspawned enum to not spawned
        public Enemy(Texture2D newTexture, int health, int strength, Vector2 newPosition, int newWidth, int newHeight, int frames, int newspawntimer)//this constructor uses the texture, health, strength, position, width, height, and frames in the animation, as well as the spawn timer to make a new enemy
        {
            // this sets all the variables of the enemy to the constructors input
            spawntimer = newspawntimer;
            Health = health;
            MaxHealth = health;
            Strength = strength;
            texture = newTexture;
            Width = newWidth;
            Height = newHeight;
            Position = newPosition;
            rectangle = new Rectangle((int)newPosition.X, (int)newPosition.Y, Width, Height);//creates a new rectangle using the position, width, and height
            Frames = frames;
        }

        public void Update(GameTime gameTime, Map MAP, PlayerStats player)//this is the update method and takes in gameTime to determine time elapsed, the MAP for its width and height and the player for all their information
        {
            
            switch (currentSpawned)// this switch checks current spawned to see how it should update the sprite
            {

                case Spawned.Not://if its not spawned
                    {
                        exist++;// it will just add 1 to exist untill it reaches spawn timer,
                        if (exist > spawntimer) currentSpawned = Spawned.Is;// it will then set currentspawned state to is
                        break;
                    }
                case Spawned.Is:// if it is spawned
                    {
                        if (Health <= 0)// if its health is less than or equal to zero
                        {
                            Position = new Vector2(rng.Next(75,MAP.Width-75), rng.Next(0,MAP.Height-75));// it will give it a new random location within the borders of the map
                            player.experience = MaxHealth;//add experience to the player of the sae value of the Enemies health
                            player.mana = MaxHealth/2 + player.intelligence;//add mana to the player bsaed on the monsters health and the players intelligence
                            MaxHealth += 10;//increase the monsters max health
                            Health = MaxHealth;// set the monsters health to its new max health
                            
                        }
                        if (player.experience >= player.Level * 100)// if the players experience is greater than the players level*100 it will level up the player
                        {
                            player.LevelUp();
                        }
                        Move(gameTime);//This moves the sprite in the move method
                        rectangle = new Rectangle(currentFrame * Width, 0, Width, Height);// this updates the rectangle of the sprite to display the current frame that wouldve been updated in move()
                        origin = new Vector2(rectangle.Width / 2, rectangle.Height / 2);// sets the origin to always be the width and height /2 of the rectangle
                        Position += velocity;// adds velocity to the position which wouldve been updated in the move method
                        POSRect = new Rectangle((int)Position.X - Width / 2, (int)Position.Y - Height / 2, Width, Height);// makes a new rectangle at the Position of the enemy for use in collision detection
                        if (hitTimer < 0)//if the hit timer is less than 0 it will set hit back to false and the hitimer to 0
                        {
                            hit = false;
                            hitTimer = 0;
                        }
                        else//otherwise it will subtract 1 from hittimer
                        {
                            hitTimer--;
                        }
                        break;
                    }
           
        }

        }
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)//this method is used for drawing the sprites
        {
            if (currentSpawned == Spawned.Is)// if it is spawned
            {
                if (hit == false)// and hit is false
                spriteBatch.Draw(texture, Position, rectangle, Color.White, 0f, origin, 1.0f, SpriteEffects.None, 0);// the tint will be white, aka nothing
                else if (hit ==true) spriteBatch.Draw(texture, Position, rectangle, Color.DarkRed, 0f, origin, 1.0f, SpriteEffects.None, 0);// if it was hit it will be red


                spriteBatch.DrawString(font, Health.ToString(), new Vector2(POSRect.X + 6, POSRect.Y - 14), Color.White);// this will always draw its health above its head
            }
        }

        public override void Jump()//this is different from the parent because the slime can jump slightly less than the player
        {

            velocity.Y = -15f;//sends it upwards
            Jumped = true;// sets jump to true so that it cant jump again
        }
        public void Move(GameTime gameTime)// this move method moves the slime by changing its velocity
        {
            

            movetimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 2;// counts how many milliseconds have passed 
            if (movetimer > moveinterval)// if the milliseconds passed is larger tahn the wait interval
            {
                this.action = rng.Next(0, 100);//it will make a new random action
                movetimer = 0;// and set the movetime back to 0
            }
            if (this.action < 33)//if the rng was less than 33, it will move right just like a player
            {
                AnimateRight(gameTime);//this animates it so it moves right
                velocity.X = 4;// this actually makes it move right
            }
            else if (this.action > 33 && action < 66)//if its between 33 and 66
            {
                AnimateLeft(gameTime);// it moves left
                velocity.X = -4;
            }
            else// if its neither of those two it will slowly stop moving
            {
                velocity.X = (float)Math.Round((velocity.X * 0.85), 2);// by rounding the number it ensures that it is slowly stopping

                if (velocity.X > 0)//if the velocity when they let go was greater than zero
                {
                    currentFrame = Frames/2-1;// it will set it to the idle frame of moveing right
                    if (velocity.X < 0.05)// if the velocity gets near 0, it will set it to zero, because rounding cant quite get there
                    {
                        velocity.X = 0f;
                    }
                }
                if (velocity.X < 0)// if the velocity was less than zero, meaning they were moving left
                {
                    currentFrame = Frames / 2;// sets it to the idle frame of moving left
                    if (velocity.X > -0.05)//like the previous, it rounds off their velocity
                    {
                        velocity.X = 0f;
                    }
                }

            }
            if (Jumped == true)//if jumped is true(this is extra gravity for the jump)
            {
                float i = 1;
                velocity.Y += 0.15f * i;//adds this extra gravity to slow down the jump

            }
            if (velocity.Y >= 20)//stops them from going past the terminal Y velocity of 20
            {
                velocity.Y = 20;//sets velocity.y to 20 when they are going faster than 20
            }

            if (Jumped == false && action > 66)//if they havent jumped, and the RNG is set to jump, it will
            {
                Jump();// jump
                movetimer = moveinterval;//and it will instantly send the movetimer to be = to the interval so that it immediately picks a new action
            }

            if (velocity.Y < 20)//This is the constant of gravity, it will run as long as velocity is less than the terminal velocity of 20
                velocity.Y += 0.4f;//gravity, is a factor of 0.4f

        }
        public void CollisionPlayer(PlayerStats player)//This method checks collision between the enemy and the player
        {
            if (player.health <= 0)//if the enemy kills the player, it will 
            {
                player.health = player.maxHealth;//reset the players health to full
                player.Position = new Vector2(250, 0);//and respawn the player at the beggining
            }

            if (POSRect.Intersects(player.POSRect) && player.hit == false)// if the Position rectangle of the enemy intersects that of the players, and the player wasnt hit recently
            {
                player.health = -Strength;// it will add -strength to the players health. the reson it is just = is because of encapsulation andi have it as set{Health += value;} so i dont have to do the plus
                if (velocity.X > 0)// if the enemy was moving  right
                {
                    player.hit = true;// sets my hit boolean to true so i cant get hit again
                    player.hitTimer = 60;//sets the timer for reseting the player getting hit to 60
                    player.velocity.X += 10;//sets the players velocity to move slightly right
                    player.velocity.Y -= 10;// and knocks the player slightly up



                }
                else if (velocity.X <= 0)//if the velocity is lessthan or equal to 0 meaning the enemy is moving left
                {
                    player.hit = true;// once again resets the boolean to true
                    player.hitTimer = 60;// and the timer before the boolean is set to true to 60
                    player.velocity.X -= 10;//and knocks the player left
                    player.velocity.Y -= 10;// and up


                }
            }

        }
        public void SlimeCollision(Sword sword, PlayerStats player)// this checks to see if the enemy is colliding with a sword
        {
            if (sword.POSRect.Intersects(POSRect) && sword.swinged == true && hit == false)//if the swords rectangle is intersecting the slimes, and the sword is being swung, and the slime hasnt been hit recently
            {

                Health += -player.strength;//the slimes health gets the players strength taken out of it
                if (sword.SWUNG == Sword.swung.left)// if the sword was swung to the left
                {
                    action = 40;//sets the slime action to 40, making it run away to the left
                    movetimer = 0;//sets movetimer to 0 so that it doesnt instantly turn back around
                    
                }
                if (sword.SWUNG == Sword.swung.right)// this is the same but makes the action run to the right
                {
                    action = 20;
                    movetimer = 0;
                    
                }
                hit = true;//sets the slimes hit boolean to true
                hitTimer = 60;// and the time before it can get hit again to 60
            }
        }
    }
}
