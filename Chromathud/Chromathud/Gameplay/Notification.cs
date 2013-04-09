using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChromathudWin.Gameplay
{
    class Notification : MiniGameComponent
    {
        //the image to swn
        private string filename = null;
        private string text;
        private Rectangle location;
        protected float alpha;
        protected Texture2D image;


        public Rectangle Location
        {
            get { return location; }
            set { location = value; }
        }

        public Notification(GameFacet facet, Rectangle location, string text)
            : base(facet)
        {
            this.text = text;
            this.location = location;
            alpha = 255;
        }
        public Notification(GameFacet facet, string filename, Rectangle location)
            : base(facet)
        {
            this.filename = filename;
            this.location = location;
            alpha = 255;

        }

        public override void Initialize()
        {
            base.Initialize();
            LoadContent();
        }

        public void LoadContent()
        {
            if (filename != null)
            {
                image = Content.LoadImage(filename);

                //try not to stretch the image
                if (location.Height != image.Height * location.Width / image.Width)
                    location.Height = image.Height * location.Width / image.Width;
            }
        }
        public override void Draw(GameTime gameTime)
        {
            Color col = new Color((byte)alpha, (byte)alpha, (byte)alpha, (byte)alpha);
            if (image != null)
                SpriteBatch.Draw(image, location, null, col);
            if (text != null)
                SpriteBatch.DrawTextScaled(location, text, 0, 0, col);
        }
        public override void Update(GameTime gameTime)
        {
            alpha *= (60.25F / 61.0F);
            if (alpha < 32.0F)
            {
                Alive = false;
            }
            
        }
    }
}
