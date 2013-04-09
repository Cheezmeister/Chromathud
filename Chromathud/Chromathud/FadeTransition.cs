using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Chromathud;

namespace ChromathudWin
{
    class FadeTransition : FacetTransition, IDisposable
    {
        private double fade;
        private double millis;
        private double start;
        private double elapsed;

        private RenderTarget2D[] renderTarget = new RenderTarget2D[2];

        public FadeTransition(int milliseconds)
            : base() 
        {
            millis = milliseconds;
        }
        public override void Setup(IGameFacet from, IGameFacet to)
        {
            base.Setup(from, to);
            fade = 1.0F; //start at 0 alpha
            start = -1;
            renderTarget[0] = new RenderTarget2D(SpriteBatch.GraphicsDevice, SpriteBatch.GraphicsDevice.Viewport.Width, SpriteBatch.GraphicsDevice.Viewport.Height);
            renderTarget[1] = new RenderTarget2D(SpriteBatch.GraphicsDevice, SpriteBatch.GraphicsDevice.Viewport.Width, SpriteBatch.GraphicsDevice.Viewport.Height);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (start == -1)
                start = gameTime.TotalGameTime.TotalMilliseconds;

            elapsed = (gameTime.TotalGameTime.TotalMilliseconds - start);
            fade = 1.0 - (float)((float)elapsed / (float)millis);

            if (fade < 0)
                Finished = true;
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {

            byte alpha = (byte)((1 - fade) * 255.0F);
            SpriteBatch.GraphicsDevice.SetRenderTarget(renderTarget[0]);
            oldFacet.Draw(gameTime);
            SpriteBatch.End();

            SpriteBatch.GraphicsDevice.SetRenderTarget(renderTarget[1]);
            SpriteBatch.Begin();
            newFacet.Draw(gameTime);
            SpriteBatch.End();
            
            SpriteBatch.GraphicsDevice.SetRenderTarget(null);
            SpriteBatch.Begin();
            SpriteBatch.Draw(renderTarget[0], Vector2.Zero, Color.White);
            SpriteBatch.Draw(renderTarget[1], Vector2.Zero, new Color(alpha, alpha, alpha, alpha));
        }

        public void Dispose()
        {
            renderTarget[0].Dispose();
            renderTarget[1].Dispose();
        }
    }
}
