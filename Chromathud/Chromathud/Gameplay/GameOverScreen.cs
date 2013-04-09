using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChromathudWin.Gameplay
{
    class GameOverScreen : GameFacet
    {
        private Texture2D logo;
        private Texture2D background;
        private Texture2D frame;
        private List<Score.Entry> topscores;
        private string enteredName;
        private const int NAME_LENGTH = 12;
        private char nextChar = 'A';

        public Score Score
        {
            get;
            set;
        }
        public GameOverScreen(Field lostGame)
            : base(lostGame.Facet.FacetManager)
        {
            Score = lostGame.Score;
            Player = lostGame.Player.Value;
        }

        public override void LoadContent()
        {
            logo = Content.LoadImage("mnu/GameOver");
            background = Content.LoadImage("TileImages/BGTile");
            frame = Content.LoadImage("TileImages/LeaderboardFrame");
            topscores = Score.CheckLeaderboard(((ChromathudGame)FacetManager.Game).SharedSaveDevice);
            enteredName = Util.GetPlayerName(Player);
        }

        public override void UnloadContent()
        {
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {

            if (InputManager.GetBoolean(UserCommand.SceneSkip) ||
                (InputManager.GetBoolean(UserCommand.SceneNext)) && !Score.Changed)
            {
                if (CumulativeTime.TotalSeconds > 2)
                {
                    Exit();
                }
            }

            // Handle name entry
            if (Score.Changed)
            {
                HandleNameEntry();
            }

            base.Update(gameTime);
        }

        private void HandleNameEntry()
        {
            foreach (Keys k in Keyboard.GetState().GetPressedKeys())
            {
                //check key was pressed and not just being held
                if (!KeyboardDelta.WasKeyPressed(k))
                    continue;

                if (k == Keys.Back && enteredName.Length > 0)
                {
                    enteredName = string.Empty;
                }
                if (k >= Keys.A && k <= Keys.Z && enteredName.Length <= NAME_LENGTH)
                {
                    enteredName += k;
                    SoundManager.PlaySound(SFX.Tick, false);
                }
            }

            GamePadDelta gpd = GamePadDelta.ForPlayer(InputManager.ActivePlayer);
            if (gpd.WasButtonPressed(Buttons.DPadUp))
            {
                if (nextChar < 'Z')
                    nextChar++;
                SoundManager.PlaySound(SFX.Tick, false);
            }
            else if (gpd.WasButtonPressed(Buttons.DPadDown))
            {
                if (nextChar > 'A')
                    nextChar--;
                SoundManager.PlaySound(SFX.Tick, false);
            }
            if (gpd.WasButtonPressed(Buttons.B) || gpd.WasButtonPressed(Buttons.DPadLeft))
            {
                if (enteredName.Length > 0)
                {
                    enteredName = enteredName.Substring(0, enteredName.Length - 1);
                }
            }
            else if (gpd.WasButtonPressed(Buttons.A) || gpd.WasButtonPressed(Buttons.DPadRight))
            {
                enteredName += nextChar;
            }
        }
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Rectangle tsa = SpriteBatch.GraphicsDevice.Viewport.TitleSafeArea;

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
            Rectangle logodest = logo.FitIn(tsa, 0, -1);
            logodest.Y -= 48; //HACK!

            if (topscores == null)
            {
                topscores = Score.CheckLeaderboard(((ChromathudGame)FacetManager.Game).SharedSaveDevice);
            }
            if (topscores == null)
            {
                Rectangle line = SpriteBatch.GraphicsDevice.Viewport.TitleSafeArea;
                SpriteBatch.DrawTextScaled(line, "No leaderboard available", 0, 1);
                return;
            }
            else //draw the high score list if available
            {
                
                Rectangle sdest = Util.CenterIn(frame, tsa);

                sdest.Inflate(0, (sdest.Top - logodest.Bottom) * 6 / 10);

                Rectangle line = sdest;
                line.Height /= Score.HIGH_SCORES_SAVED + 1;

                SpriteBatch.Draw(frame, sdest, Color.White);
                SpriteBatch.DrawTextScaled(line, "Top Scores:", 0, -1);
                line.Y += line.Height;
                
                for (int i = 0; i < topscores.Count; ++i )
                {
                    Score.Entry e = topscores[i];
                    color = (i == Score.LeaderboardRank) ? Color.ForestGreen : Color.White;

                    Rectangle left = line;
                    left.Width *= 3;
                    left.Width /= 10;

                    SpriteBatch.DrawTextScaled(left, e.score.ToString() + ' ', 1, 0);

                    string str = e.name;
                    if (Score.Changed && i == Score.LeaderboardRank)
                    {
                        e.name = enteredName;
                        str = ' ' + e.name;
                        if (this.CumulativeTime.Milliseconds % 200 > 100)
                            str += (Util.UseXboxUI() ? nextChar.ToString().Replace('@', ' ') : "_");
                        else
                            str += " ";
                    }
                    Rectangle right = line;
                    right.X = left.Right;
                    right.Width *= 7;
                    right.Width /= 10;

                    SpriteBatch.DrawTextScaled(right, str, -1, 0, color);
                    line.Y += line.Height;
                }
            }
            SpriteBatch.Draw(logo, logodest, Color.White);
            Rectangle dest = tsa;
            dest.Height += 40; // HACK!
            SpriteBatch.DrawTextScaled(dest, "Your score: " + Score.VisibleTotal.ToString(), 0, 1);
        }
        public override void Exit()
        {
            if (Score.LeaderboardRank >= 0)
            {
                Score.Entry e = topscores[Score.LeaderboardRank];
                e.name = enteredName;
                topscores[Score.LeaderboardRank] = e;

                Score.SaveLeaderboard(((ChromathudGame)FacetManager.Game).SharedSaveDevice);
            }
            base.Exit();

        }

        public PlayerIndex Player { get; set; }
    }
}
