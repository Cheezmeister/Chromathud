using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Chromathud;

namespace ChromathudWin.Gameplay
{
    class MultiplayScreen : GameFacet
    {
        Texture2D logo, background, frame, loadLan, loadInet, uncheckedBox, checkedBox, sessionBg, sessionBgSelected;
        Rectangle loadLanBtn, loadInetBtn;
        public MultiplayScreen(FacetManager fm)
            : base(fm)
        {
        }

        public override void LoadContent()
        {
            logo = Content.LoadImage("mnu/Multiplayer");
            background = Content.LoadImage("TileImages/BGTile");
            frame = Content.LoadImage("TileImages/LeaderboardFrame");
            sessionBg = Content.LoadImage("TileImages/36/Timer");
            sessionBgSelected = Content.LoadImage("TileImages/44/Timer");
        }

        private void startGame()
        {
            Exit();
            //FacetManager.AddFacet(game);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (InputManager.GetBoolean(UserCommand.SceneSkip) )
            {
                Exit();
            }
        }

        public override void UnloadContent()
        {
        }

        public override void Draw(GameTime gameTime)
        {
            Color color = Color.White;

            //draw the tiled background
            Vector2 location = new Vector2();
            for (int x = 0; x < FacetManager.GraphicsDevice.Viewport.Width; x += background.Width)
            {
                location.X = x;
                for (int y = 0; y < FacetManager.GraphicsDevice.Viewport.Height; y += background.Height)
                {
                    location.Y = y;
                    SpriteBatch.Draw(background, location, color);
                }
            }

            //draw logo top & centered
            Rectangle tsa = SpriteBatch.GraphicsDevice.Viewport.TitleSafeArea;
            Rectangle dest = logo.FitIn(tsa, 0, -1);
            SpriteBatch.Draw(logo, dest, Color.White);

            Rectangle line = dest
                    = Util.CenterIn(frame, tsa);
            line.Height = Util.GlobalFont.LineSpacing;

            SpriteBatch.Draw(frame, dest, color);

            int numSessions = 4;
            for (int i = 0; i < numSessions; i++)
            {
                SpriteBatch.Draw(sessionBg, line, color);
                line.Y += line.Height;
            }
        }
    }
}
