using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuessaria
{
    class Camera
    {
        public Matrix transform;// this is the camera transform matrix, it is very confusing and i have only a basic knowledge of it
        Viewport view;// this is the viewport in which the game is displayed
        Vector2 centre;// this is the centre of that viewport
        public Vector2 centrer = new Vector2();
        public Vector2 CENTRE//this is a encapsulation to return the centre to where its called
        {
            get { return centre; }
        }
        public Camera(Viewport NewView)// this is the camera constructor that sets the viewpoint from the graphics device
        {
            view = NewView;
        }
        public void Update(GameTime gameTime, PlayerSprite Player, int width, int height, Rectangle rectangle)// this is used for updateting the camera and takes in the playersprite so it cantrack the player
        {
            centre = new Vector2(Player.Position.X , Player.Position.Y);// this is the centre of the camera and will always be at the players position
            
            float left = centre.X - rectangle.Width / 2;
            float top = centre.Y - rectangle.Height * 2 / 3;
            float right = centre.X + rectangle.Width / 2;
            float bottom = centre.Y + rectangle.Height / 3;

            if (left <= 0)
                centrer.X = rectangle.Width/2;
            else if (right >= width)
                centrer.X = width - rectangle.Width/2;
            else
                centrer.X = centre.X;// it just follows the characters x

            if (top <= 0)
                centrer.Y = rectangle.Height*2/3;
            else if (bottom >= height)
                centrer.Y = height - rectangle.Height/3;
            else
                centrer.Y = centre.Y;// and y



            transform = // this is the camera transformation matrix, i watched a 3 hour lecture and vaguely understand whats going on here
                Matrix.CreateTranslation(new Vector3(-centrer.X, -centrer.Y, 0)) *//so it creates a matrix translation using the inverse of the center(because cameras take in light upsidedown or something like that) then it multiplies it by the next line
                Matrix.CreateScale(new Vector3(0.75f, 0.75f, 0)) * //this creates a scale for the transormation. This scale is 0.75times the size of the actual game it then multiplies it by the next line
                Matrix.CreateTranslation(new Vector3(view.Width/2, view.Height*2/3, 0));//this creates the translation so it can follow the player but not in the top left but rather in the centre of the viewport

        }

    }
}
