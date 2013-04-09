using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ChromathudWin
{
    /// <summary>
    /// Fades, wipes, dissolves, and the like
    /// </summary>
    public abstract class FacetTransition
    {
        protected IGameFacet oldFacet;
        protected IGameFacet newFacet;

        /// <summary>
        /// Was setup already performed? True in some weird cases.
        /// </summary>
        public bool IsSetup { get; private set; }

        public IGameFacet From { get; private set; }
        public IGameFacet To { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }

        public AsyncCallback OnCleanup;

        public bool Finished { get; protected set; }
        public FacetTransition()
        {
        }
        public FacetTransition(IGameFacet from, IGameFacet to)
        {
            Setup(from, to);
        }

        public virtual void Setup(IGameFacet from, IGameFacet to)
        {
            if (IsSetup)
                return;

            oldFacet = from;
            newFacet = to;
            SpriteBatch = oldFacet.FacetManager.SpriteBatch;
            
            IsSetup = true;

        }
        public virtual void Cleanup()
        {
            if (OnCleanup != null)  
                OnCleanup(null);
        }

        public abstract void Update(Microsoft.Xna.Framework.GameTime gameTime);
        public abstract void Draw(Microsoft.Xna.Framework.GameTime gameTime);

    }
}
