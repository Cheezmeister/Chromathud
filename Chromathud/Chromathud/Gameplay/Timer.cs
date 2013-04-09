using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChromathudWin.Gameplay
{
    class Timer : MiniGameComponent
    {
        private Texture2D graphic;
        private int Padding
        {
            get
            {
                return Layout.TimerPadding;
            }
        }
        private Rectangle _area;

        public Rectangle Area
        {
            get { return _area; }
            set { _area = value; }
        }

        private Point center;
        public Point Center 
        {
            get
            {
                return center;
            }
            set
            {
                center = value;
                Area = new Rectangle(Center.X - graphic.Width / 2, Center.Y - graphic.Height / 2, graphic.Width, graphic.Height);
            }
        }
        
        new private GameplayFacet Facet
        {
            get { return (GameplayFacet)base.Facet; }
        }
        public Timer(GameplayFacet facet)
            : base(facet)
        {
            graphic = Content.LoadImage("TileImages/" + Block.Width + "/Timer");
            _area.Width = graphic.Width;
            _area.Height = graphic.Height;
        }

        public override void Update(GameTime gameTime)
        {
            
        }
        public override void Draw(GameTime gameTime)
        {

            Rectangle dest = Area;

            SpriteBatch.Draw(graphic, dest, Color.White);
            
            
            dest.Inflate(-Padding, 0);
            string str = 
                string.Format("{0:00}", Facet.ClockTime.Minutes) +
                ":" +
                string.Format("{0:00}", Facet.ClockTime.Seconds);


            // Draw characters separately so that they appear monospaced
            dest.Height /= 2;
            dest.Y += dest.Height / 2;
            dest.Width /= 5;
            foreach (char c in str)
            {
                SpriteBatch.DrawTextScaled(dest, c.ToString(), 1, 0);
                dest.X += dest.Width;
            }
        }
    }
}
