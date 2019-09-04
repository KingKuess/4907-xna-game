using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuessaria
{
    class Tiles// this is the tiles the character stands on
    {
        protected Texture2D texture;// this is the texture of the tile

        private Rectangle rectangle;//this is the rectangle the triangle is at
        public Rectangle Rectangle
        {
        get { return rectangle; }//this is so its publicly getable
            protected set { rectangle = value; }//and protected setable
        }

        private static ContentManager content;//this is so I can load content through the class
        public static ContentManager Content//this makes the content public
        {
            protected get { return content; }
            set { content = value; }
        }

        public void Draw(SpriteBatch spriteBatch, Color color)// this draws the tile
        {
            spriteBatch.Draw(texture, rectangle, color);
        }
        

    }
    class CollisionTiles : Tiles//this is the collisiontiles class
    {
        public CollisionTiles(int i, Rectangle newRectangle)//it is made differently in that its texture is taken from an int
        {
            texture = Content.Load<Texture2D>("Blocks/Tile" + i);//it adds i onto the end of the filename to get either tile 1 2 or 3
            this.Rectangle = newRectangle;//and it sets the rectangle to the rectangle submitted
        }
    }
}
