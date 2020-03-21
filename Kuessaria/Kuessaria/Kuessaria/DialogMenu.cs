using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;

namespace Kuessaria
{
    class DialogMenu
    {
        public string text;
        public Buttons ok;
        public Buttons close;
        bool visible;
        Texture2D back;
        SpriteFont font;


        public DialogMenu(string Text, SpriteFont Font, Texture2D Back, Buttons Ok, Buttons Close)
        {
            text = Text;
            visible = false;
            back = Back;
            font = Font;

            ok = Ok;
            close = Close;
        }
        public void Update(MouseState mouse, Vector2 centre, Rectangle rectangle)
        {

                ok.setPOS(new Vector2(centre.X + rectangle.Width / 2 - 1000 + 200, (centre.Y - rectangle.Height * 2 / 3 + 120 + 550)));
                close.setPOS(new Vector2(centre.X + rectangle.Width / 2 - 1000 + 400, (centre.Y - rectangle.Height * 2 / 3 + 120 + 550)));

                ok.setViewBasedPOS(new Vector2(1450, 410));
                close.setViewBasedPOS(new Vector2(1590, 410));

                ok.Update(mouse);
                close.Update(mouse);
                if (close.isclicked)
                {
                    visible = false;
                }
                if (ok.isclicked)
                {
                    visible = false;
                }
            
        }
        public void Draw(SpriteBatch spriteBatch, Viewport Viewport, Vector2 centre, Rectangle rectangle)
        {
            if (visible)
            {
                spriteBatch.Draw(back, new Vector2(centre.X + rectangle.Width / 2 - 1000, (centre.Y - rectangle.Height * 2 / 3 + 120)), Color.White * 0.8f);

                spriteBatch.DrawString(font, text, new Vector2(centre.X + rectangle.Width / 2 - 1000 + 15, (centre.Y - rectangle.Height * 2 / 3 + 120 + 15)), Color.White);

                ok.Draw(spriteBatch);
                close.Draw(spriteBatch);
            }
        }
        public void Show(string message)
        {
            text = message;
            visible = true;
        }
    }
}
