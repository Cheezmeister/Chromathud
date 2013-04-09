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


namespace ChromathudWin
{
    /// <summary>
    /// A game facet is essentially any part of the game that has its own 
    /// event loop, i.e. Menu, Gameplay, Transition
    /// </summary>
    public interface IGameFacet : IGameComponent
    {
        void Update(Microsoft.Xna.Framework.GameTime gameTime);
        void Draw(Microsoft.Xna.Framework.GameTime gameTime);
        void Cleanup();
        void Exit();
        FacetManager FacetManager { get; }
        TimeSpan CumulativeTime { get; }
    }
    
    /// <summary>
    /// Basic IGameFacet implementation
    /// </summary>
    public abstract class GameFacet : IGameComponent, ChromathudWin.IGameFacet
    {
        private FacetManager fm;
        private TimeSpan? started = null;

        #region properties
        public TimeSpan CumulativeTime { get; private set; }

        public FacetManager FacetManager
        {
            get { return fm; }
        }
        protected ContentManager Content
        {
            get { return fm.Content; }
        }
        protected SpriteBatch SpriteBatch
        {
            get { return fm.SpriteBatch; }
        }
        #endregion

        public GameFacet(FacetManager fm)
        {
            this.fm = fm;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public virtual void Initialize()
        {
            LoadContent();
        }

        /// <summary>
        /// Tidy up afterwards
        /// </summary>
        public virtual void Cleanup()
        {
            UnloadContent();
        }
        
        /// <summary>
        /// Similar to LoadContent() for GameComponent
        /// </summary>
        public abstract void LoadContent();

        public abstract void UnloadContent();

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update(GameTime gameTime)
        {
            if (started == null)
                started = gameTime.TotalGameTime;

            CumulativeTime = gameTime.TotalGameTime - started.Value;
        } 
          
        /// <summary>
        /// Draw whatever needs to be drawn
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Draw(GameTime gameTime);

        /// <summary>
        /// Exit this facet, going back to the previous one
        /// </summary>
        public virtual void Exit()
        {
            FacetManager.PopFacet();
        }
      
    }
}