using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace ChromathudWin.Gameplay
{
    public interface IMiniGameComponent
    {

    }

    /// <summary>
    /// A class designed to mimic XNA's GameComponent with less overhead
    /// </summary>
    public abstract class MiniGameComponent : IMiniGameComponent
    {
        private IGameFacet facet;
        private bool alive;
        private bool enabled;
        #region Properties
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
        /// <summary>
        /// When false, this component should be removed from its parent component
        /// </summary>
        public bool Alive
        {
            get { return alive; }
            set { alive = value; }
        }
        public IGameFacet Facet
        {
            get { return facet; }
            set { facet = value; }
        }
        public ContentManager Content
        {
            get;
            private set;
        }
        public SpriteBatch SpriteBatch
        {
            get;
            private set;
        }
        #endregion

        public MiniGameComponent(IGameFacet facet)
        {
            this.facet = facet;
            alive = enabled = true;
            this.Content = facet.FacetManager.Content;
            this.SpriteBatch = facet.FacetManager.SpriteBatch;
        }
        /// <summary>
        /// Initialize
        /// </summary>
        public virtual void Initialize()
        {
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);
        /// <summary>
        /// Perform rendering
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Draw(GameTime gameTime);


    }
}
