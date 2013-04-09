using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using WickedLibrary.Graphics;
using WickedLibrary.Graphics.ParticleSets;
using Microsoft.Xna.Framework.Content;
using ChromathudWin.Gameplay;

namespace ChromathudWin
{
    /// <summary>
    /// A small facet before starting the game to detect the active player
    /// </summary>
    class SplashScreen : GameFacet
    {
        private enum State
        {
            LogoFlash,
            LogoOff,
            Normal,
        }

        private const int LOGO_MILLIS = 4000;

        Texture2D bgtile;
        Texture2D title;
        Texture2D teamlogo;
        Color pulseColor = Color.Transparent;
        List<FallingThing> fallingthings;
        private string msg = Chromathud.Strings.Loading;
        private double flashalpha = 0.0;

        State state = State.LogoFlash;

        public SplashScreen(FacetManager fm)
            : base(fm)
        {
            fallingthings = new List<FallingThing>();
            for (int i = 0; i < Field.NumColumns; ++i)
            {
                fallingthings.Add(new FallingThing(this, i));
            }
        }
        public override void LoadContent()
        {
            bgtile = Content.LoadImage("TileImages/BGTile");
            title = Content.LoadImage("mnu/Title");
            teamlogo = Content.LoadImage("mnu/chromateam");
        }

        public override void UnloadContent()
        {
            //bgtile.Dispose();  
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Rectangle tsa = FacetManager.GraphicsDevice.Viewport.TitleSafeArea;

            flashalpha = 1 - 2 * Math.Abs(CumulativeTime.TotalMilliseconds / LOGO_MILLIS - 0.5);
            
            if (state == State.LogoFlash)
            {
                if (CumulativeTime.TotalMilliseconds > LOGO_MILLIS)
                {
                    state = State.Normal;
                }
                else if (CumulativeTime.TotalMilliseconds > LOGO_MILLIS / 2)
                { 
                    SoundManager.PlaySound(SFX.Thump, true);
                    state = State.LogoOff;
                }  

                FacetManager.GraphicsDevice.Clear(Color.Black);
                Color col = Color.White.MakeTranslucent((byte)(flashalpha * 255.0));
                SpriteBatch.Draw(teamlogo, teamlogo.CenterIn(tsa), col);

                return;
            }

            //background
            for (int i = 0; i <= FacetManager.GraphicsDevice.Viewport.Width / bgtile.Width; ++i)
            {
                for (int j = 0; j <= FacetManager.GraphicsDevice.Viewport.Height / bgtile.Height; ++j)
                {
                    Vector2 pos = new Vector2(i * bgtile.Width, j * bgtile.Height);
                    SpriteBatch.Draw(bgtile, pos, Color.White);
                }
            }

            //falling stuff
            for (int i = 0; i < fallingthings.Count; ++i)
            {
                fallingthings[i].Draw(gameTime);
            }

            //title
            Rectangle dest = Util.FitIn(title, tsa, 0, -1);
            SpriteBatch.Draw(title, dest, Color.White);

            //press start flasher
            dest = tsa;
            dest.Width /= 3;
            dest.X += dest.Width;
            SpriteBatch.DrawTextScaled(dest, msg, 0, 1, pulseColor);

            if (state == State.LogoOff)
            {
                if (CumulativeTime.TotalMilliseconds > LOGO_MILLIS)
                {
                    state = State.Normal;
                }
                else
                {
                    SpriteBatch.DrawRectangle(FacetManager.GraphicsDevice.Viewport.Bounds, Color.Black.MakeTranslucent((byte)(flashalpha * 255.0)));
                    Color col = Color.White.MakeTranslucent((byte)(flashalpha * 255.0));
                    SpriteBatch.Draw(teamlogo, teamlogo.CenterIn(tsa), col);
                }
            }

        }
        public override void Update(GameTime gameTime)
        {
            bool start = false;
#if XBOX
            for (PlayerIndex p = PlayerIndex.One; p <= PlayerIndex.Four; ++p)
            {
                if (GamePadDelta.ForPlayer(p).WasButtonReleased(Microsoft.Xna.Framework.Input.Buttons.Start) ||
                    GamePadDelta.ForPlayer(p).WasButtonReleased(Microsoft.Xna.Framework.Input.Buttons.A))
                {
                    InputManager.ActivePlayer = p;
                    start = true;
                }
            }
#else
            if (InputManager.GetBoolean(UserCommand.MenuStart) ||
                InputManager.GetBoolean(UserCommand.MenuSelect) ||
                InputManager.GetBoolean(UserCommand.SceneNext) )
                start = true;
#endif
            if (Preferences.Valid)
            {
                msg = Chromathud.Strings.PressStart;
                if (start)
                {
                    FadeTransition t = new FadeTransition(600);
                    Menu.GameMenu menu = new Menu.MainMenu(FacetManager);
                    t.Setup(this, menu);
                    FacetManager.ReplaceMeWith(menu, t);
                }
            }

            int pulseMillis = 2000;
            double angle = (double)CumulativeTime.TotalMilliseconds * 2 * Math.PI / pulseMillis;
            pulseColor.G = (byte)(128 + 100.0 * Math.Sin(angle));

            for (int i = 0; i < fallingthings.Count; ++i)
                fallingthings[i].Update(gameTime);

            base.Update(gameTime);
        }

        private class FallingThing 
        {
            Vector2 center;
            Rectangle area;
            Texture2D sprite;
            SplashScreen facet;
            float speed;
            private static bool[] cols = new bool[20];
            public FallingThing(SplashScreen facet, int column)
            {
                int num = Util.Random.Next(9) + 1;
                sprite = facet.Content.LoadImage("Nova3/" + num);
                area.Width = sprite.Width;
                area.Height = sprite.Height;
                area.X = column * Block.Width;
                this.facet = facet;
                this.reset();
            }
            public void Draw(GameTime gameTime)
            {
                facet.SpriteBatch.Draw(sprite, area, Color.White);
            }
            public void Update(GameTime gameTime)
            {
                center.Y += speed;

                area.Y = (int)center.Y - area.Height / 2;
                if (area.Y > 755)
                    reset();
            }
            private void reset()
            {
                speed = Util.Random.NextFloat() * 4 + 0.5F;
                
                //pick a new (unoccupied) column\
                area.X /= Block.Width;
                cols[area.X] = false;
                do
                    area.X = Util.Random.Next(755 / Block.Width);
                while (cols[area.X]);
                cols[area.X] = true;
                area.X *= Block.Width;

                area.Y = 0 - Util.Random.Next(700);
                center.Y = area.Y + area.Height / 2;
                center.X = area.X + area.Width / 2;
            }

        }
    }
}
