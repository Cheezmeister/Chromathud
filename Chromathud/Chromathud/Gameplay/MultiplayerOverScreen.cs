using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChromathudWin.Gameplay
{
    class MultiplayerOverScreen : GameFacet
    {
        private Texture2D shadow;
        private Texture2D background;
        private Texture2D frame;
        private List<Score.Entry> topscores;
        private const int NAME_LENGTH = 12;
        private Field winner;
        private Field loser;

        public Score Score
        {
            get;
            set;
        }
        public MultiplayerOverScreen(FacetManager mgr)
            : base(mgr)
        {
        }

        public MultiplayerOverScreen(FacetManager mgr, Field winner, Field loser)
            : this(mgr)
        {
            this.winner = winner;
            this.loser = loser;
            Score = winner.Score;
        }

        public override void LoadContent()
        {
            shadow = Content.LoadImage("TileImages/Shadow");
            background = Content.LoadImage("TileImages/BGTile");
            frame = Content.LoadImage("TileImages/LeaderboardFrame");
        }

        public override void UnloadContent()
        {
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {

            if (InputManager.GetBoolean(UserCommand.SceneSkip) ||
                InputManager.GetBoolean(UserCommand.SceneNext))
            {
                if (CumulativeTime.TotalSeconds > 2)
                {
                    Exit();
                }
            }

            base.Update(gameTime);
        }
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
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
                    SpriteBatch.Draw(background, location, Color.White);
                }
            }

            //draw logo top & centered
            Rectangle tsa = SpriteBatch.GraphicsDevice.Viewport.TitleSafeArea;
            Rectangle dest = shadow.FitIn(tsa, 0, -1);
            SpriteBatch.Draw(shadow, dest, Color.White);

            string p = Util.GetPlayerName(winner.Player.GetValueOrDefault(PlayerIndex.Two));

            SpriteBatch.DrawTextScaled(dest, "A Winner Is", 0, 0, Color.Red);
            dest.Y = dest.Bottom;
            SpriteBatch.DrawTextScaled(dest, p, 0, 0, Color.Red);
            dest.Height /= 2;
            dest.Y = tsa.Bottom - dest.Height;
            SpriteBatch.DrawTextScaled(dest, "Score: " + Score, 0, 1);
        }
    }
}
