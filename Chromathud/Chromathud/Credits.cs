using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChromathudWin.Menu;

namespace ChromathudWin
{
    class Credits : GameMenu
    {
        Texture2D brendan;
        Texture2D dave;
        Texture2D ben;

        public Credits(FacetManager fm)
        :
            base(fm)
        {
        }
        protected override void AddItems() { }

        protected override void LoadContent()
        {
            background = Content.LoadImage("TileImages/BGTile");
            dave = Content.LoadImage("../dave");
            brendan = Content.LoadImage("../brendan");
            ben = Content.LoadImage("../ben");
        }

        protected override void UnloadContent()
        {
            
        }
        public override void Update(GameTime gameTime)
        {
            if (InputManager.GetBoolean(UserCommand.SceneBack) ||
                InputManager.GetBoolean(UserCommand.SceneSkip) ||
                InputManager.GetBoolean(UserCommand.MenuBack))
                Exit();
        }
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            drawBackground();

            Rectangle tsa = FacetManager.GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 dloc = Util.TopLeftCorner(tsa);
            SpriteBatch.Draw(dave, dloc, Color.White);
            Vector2 bloc = new Vector2(tsa.Right - brendan.Width, tsa.Bottom - brendan.Height);
            SpriteBatch.Draw(brendan, bloc, Color.White);
            Rectangle benloc = Util.FitIn(ben, tsa, 0, 1); // new Rectangle(tsa.Left, tsa.Bottom - ben.Height * 3 / 4, ben.Width * 3 / 4, ben.Height * 3 / 4);
            benloc.X -= 70; // HACK 
            benloc.Y -= 10;
            SpriteBatch.Draw(ben, benloc, Color.White);

            Rectangle textloc = new Rectangle((int)dloc.X + 250, (int)dloc.Y + dave.Height / 8, 1000, 80);
            SpriteBatch.DrawTextScaled(textloc, "   Art/\n  Dave Silverman\n/www.davesilvermanart.com", -1, 0);
            Rectangle btextloc = new Rectangle((int)bloc.X - 40, (int)bloc.Y + brendan.Height / 4 + 80, 1000, 80);
            SpriteBatch.DrawTextScaled(btextloc, "   Code/\n  Brendan Luchen\n/www.luchenlabs.com", -1, 0);

            int ytemp = (int)dloc.Y + dave.Height;
            ytemp = Math.Max(ytemp, btextloc.Bottom);
            ytemp = Math.Max(ytemp, tsa.Bottom - benloc.Height / 2);
            Rectangle ktextloc = new Rectangle((int)dloc.X, ytemp, (int)benloc.Width * 3 / 2, benloc.Height / 2);
            SpriteBatch.DrawTextScaled(ktextloc, "      Tunes/\n     'Chromathud': Ben Roberts\n    'Killing Time': Kevin MacLeod\n   'Short Story': Cormac Connaughton\n  /www.soundcloud.com/benandmusic\n /www.incompetech.com\n/frostbyte.bandcamp.com", -1, -1);
        }

        private void drawBackground()
        {
            //draw the tiled background
            Vector2 location = new Vector2();
            for (int x = 0; x < FacetManager.GraphicsDevice.Viewport.Width; x += background.Width)
            {
                location.X = x;
                for (int y = 0; y < FacetManager.GraphicsDevice.Viewport.Height; y += background.Height)
                {
                    location.Y = y;
                    SpriteBatch.Draw(background, location, Color.White);
                }
            }
        }
    }
}
