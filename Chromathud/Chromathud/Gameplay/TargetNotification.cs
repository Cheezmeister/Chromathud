using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ChromathudWin.Gameplay
{
    class TargetNotification : Notification
    {
        int num;

        public TargetNotification(GameplayFacet facet, int target, Rectangle location)
            : base(facet, "Target", location)
        {
            num = target;
            ////location.Width = image.Width;
            ////location.Height = image.Height;

        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            //number should be in the 3rd vertical quadrant of the image =P
            Rectangle dest = Location;
            dest.Y = dest.Center.Y;
            dest.Height /= 4;

            Color col = Color.Wheat.MakeTranslucent((byte)alpha);

            SpriteBatch.DrawTextScaled(dest, num.ToString(), 0, 0, col);
        }
    }
}
