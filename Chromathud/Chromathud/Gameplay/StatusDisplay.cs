using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace ChromathudWin.Gameplay
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class StatusDisplay : MiniGameComponent
    {
        private int shownScore = 0;

        private Point _center;
        public Point Center
        {
            get
            {
                return _center;
            }
            set
            {
                _center = value;
                if (bgScore != null && bgTarget != null)
                {
                    _area.Width = bgScore.Width;
                    _area.Height = bgTarget.Height + bgScore.Height;
                    _area.X = Center.X - _area.Width / 2;
                    _area.Y = Center.Y - _area.Height / 2;
                }
            }
        }
 
        private Rectangle _area;
        public Rectangle Area
        {
            get { return _area; }
            set { _area = value; }
        }
        public Field Field { get; private set; }
        public Score Score { get; private set; }
        
        private GameplayFacet game;

        private Texture2D bgTarget;
        private Texture2D bgLevel;
        private Texture2D bgLimit;
        private Texture2D bgScore;

        public StatusDisplay(Field field, Point center)
            : base(field.Facet)
        {
            this.Field = field;
            this.Score = field.Score;
            this.game = field.Facet;
            this.Center = center;
            Initialize();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            string p = (Field.Player == InputManager.ActivePlayer ? "" :
                Util.IsAlmostEaster() ? "/EE" :
                "/2P");
            bgTarget = Content.LoadImage("TileImages" + p + "/TargetStatus");
            bgScore = Content.LoadImage("TileImages" + p + "/ScoreStatus");

            this.Center = this._center;

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (shownScore < Score.VisibleTotal)
                ++shownScore;
        }
        /// <summary>
        /// Draw the status display
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            int offset = Layout.StatusUpperPadding;
            int ss = Layout.StatusSpacing;

            Color tint = Enabled ? Field.Player == PlayerIndex.Two ? new Color(200, 200, 200) : Color.White : Color.Gray;

            if (!Field.Enabled)
                Enabled = false;

            SpriteBatch.Draw(bgTarget, new Vector2(_area.X, _area.Y + offset), tint);
            DrawStatusString(offset, Field.Target.ToString());
            offset += ss;

            //SpriteBatch.Draw(bgLevel, new Vector2(_area.X, _area.Y + offset), tint);
            //DrawStatusString(offset, Field.Level.ToString());
            //offset += ss;

            //SpriteBatch.Draw(bgLimit, new Vector2(_area.X, _area.Y + offset), tint);
            //DrawStatusString(offset, Field.SelectLimit.ToString());
            //offset += ss;

            SpriteBatch.Draw(bgScore, new Vector2(_area.X, _area.Y + offset), tint);
            offset += bgScore.Height - ss;
            DrawStatusString(offset, shownScore.ToString());


            if (Preferences.GetBoolean("DebugMode"))
            {
                //make sure the area matches the background image
                Color c = Color.Black;
                c.A = 40;
                SpriteBatch.DrawRectangle(_area, c);

                offset += ss;
                DrawStatusString(offset, "Selection: " + Field.SelectionSize);
                offset += ss;
                DrawStatusString(offset, Field.GetTotal().ToString());
                offset += ss;
                DrawStatusString(offset, Field.BlocksAreFalling.ToString());
                offset += ss;
            }
        }
        private void DrawStatusString(int offset, string text)
        {
            DrawStatusString(offset, text, 1);
        }
        private void DrawStatusString(int offset, string text, float scale)
        {
            if (!Enabled)
                return;

            Rectangle location = new Rectangle(
                _area.X + Layout.StatusIndentation - Layout.StatusHorizontalMargin,
                _area.Y + offset + Layout.StatusVerticalMargin + Layout.StatusVerticalSkew,
                _area.Width - Layout.StatusIndentation - Layout.StatusHorizontalMargin,
                Layout.StatusSpacing - 2 * Layout.StatusVerticalMargin);

            location.Height = (int)(location.Height * scale);

            Color color = Color.White;
            color.G -= (byte)((scale - 1F) * 510F);
            color.B -= (byte)((scale - 1F) * 510F);
            SpriteBatch.DrawTextScaled(location, text, 1, 1, color);
        }

    }
}