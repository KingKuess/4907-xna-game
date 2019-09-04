using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuessaria
{
    static class RectangleHelper
    {
        public static bool TouchTop(this Rectangle r1, Rectangle r2)
        {
            return (r1.Bottom >= r2.Top - 1 &&
                r1.Bottom <= r2.Top + (r2.Height / 2) &&
                r1.Right >= r2.Left + r2.Width / 5 &&
                r1.Left <= r2.Right - r2.Width / 5);// it returns true if all of these are happening
            //basically if the player is above the top and if the playes bottom 
            //is lower than the top plus height of the block /2 and the player is inbetween the sides of the block
        }
        public static bool TouchBottom(this Rectangle r1, Rectangle r2)
        {
            return (r1.Top <= r2.Bottom + (r2.Height / 5) &&
                r1.Top >= r2.Bottom - 1 &&
                r1.Right >= r2.Left + r2.Width / 5 &&
                r1.Left <= r2.Right - r2.Width / 5);
            //its almost exactly the same as the top but checks if the palyer is below rather than above
        }
        public static bool TouchLeft(this Rectangle r1, Rectangle r2)
        {
            return (r1.Right <= r2.Right &&
                r1.Right >= r2.Left - 5 &&
                r1.Top <= r2.Bottom - (r2.Width / 4) &&
                r1.Bottom >= r2.Top + r2.Width / 4);
            //it basically checks if they are inbetween a threshold beside the block
        }

        public static bool TouchRight(this Rectangle r1, Rectangle r2)
        {
            return (r1.Left >= r2.Left &&
                r1.Left <= r2.Right +5 &&
                r1.Top <= r2.Bottom - (r2.Width / 4) &&
                r1.Bottom >= r2.Top + r2.Width / 4);
            //same as the last but on the right side
        }
    }
}
