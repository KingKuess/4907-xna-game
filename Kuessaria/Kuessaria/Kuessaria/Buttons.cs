 using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuessaria
{
    class Buttons
    {
        public Texture2D texture;//a texture that holds what the button looks like
        public Rectangle rectangle;//a rectangle that holds the buttons position,width,and height
        public Vector2 Position;// a position for the button
        public Vector2 viewBasedPOS;// a view based POS 

        Color color = new Color(255, 255, 255, 255);// colors so the button fades in and out of existence

        public Vector2 size;// a vector that is the size of the button

         public Buttons(Texture2D newTexture, GraphicsDevice graphics, int Wid,int Hei)//this constructor takes in a  texture a graphics device and a width and height
        {
            texture = newTexture;// sets the texture

            size = new Vector2(graphics.Viewport.Width / (graphics.Viewport.Width / Wid), graphics.Viewport.Height / (graphics.Viewport.Height / Hei));// sets the vector2 size using width and height
        }

        bool down;//a boolean for checking if the button colors are going down or up
        public bool isclicked;//a boolean to check if the button is clicked or not
        public void Update(MouseState mouse)// this is the update method that runs in the game1.update method
        {
            rectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)size.X, (int)size.Y);// it makes the rectangle = to the position and size
            Rectangle viewBasedRect = new Rectangle((int)viewBasedPOS.X, (int)viewBasedPOS.Y, (int)size.X, (int)size.Y);



            Rectangle mouseRectangle = new Rectangle(mouse.X, mouse.Y, 1, 1);//and makes the mouse rectangle around the mouse
            //if (mouse.LeftButton == ButtonState.Pressed)
            //{
            //  System.Windows.Forms.MessageBox.Show("Mouse POS " + mouse.X + ", " + mouse.Y + "\n" + "ButtPOS " + viewBasedPOS.X + ", " + viewBasedPOS.Y);
           // }

            if (mouseRectangle.Intersects(viewBasedRect))// it checks if the mouse rectangle and the button rectangle intersect
            {
                if (color.A == 255) down = false;//this makes colors go down if they are at the top of 255
                if (color.A == 0) down = true;// this makes colors go up if they are at the bottom, 0
                if (down) color.A += 3; else color.A -= 3;
                if (mouse.LeftButton == ButtonState.Pressed) isclicked = true;// if it gets clicked, it sets the boolean to true
            }
            else if(color.A < 255)//if the color is less than 255
            {
                color.A += 3;// it adds 3 to color
                isclicked = false;// and sets clicked to false

            }
        }
        public void setPOS(Vector2 newPOS)//a method to set the position of the button
        {
            Position = newPOS;//sets the position to the newPOS 
            viewBasedPOS = newPOS;     
        }
        public void setViewBasedPOS(Vector2 newpos)
        {
            viewBasedPOS = newpos;
        }
        public void Draw(SpriteBatch spriteBatch)// this just draws the button
        {
            spriteBatch.Draw(texture, rectangle, color);//this draw function takes in the texture, rectangle and color
        }
    }
}
