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
using System.Text.RegularExpressions;


namespace ChromathudWin
{
    /// <summary>
    /// Manage various GameFacets, such as the main menu, start menu, actual game etc.
    /// </summary>
    public class FacetManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        /// <summary>
        /// The facets currently loaded. Only the topmost is considered active
        /// </summary>
        Stack<IGameFacet> facets;

        /// <summary>
        /// A shared SpriteBatch
        /// </summary>
        SpriteBatch spriteBatch;

        //SpriteFont courierNew;
        //SpriteFont buttonFont;
        Texture2D pixel;
        private FacetTransition activeTransition;

        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }
        public ContentManager Content
        {
            get { return Game.Content; }
        }
        public FacetManager(Game game)
            : base(game)
        {
           
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            facets = new Stack<IGameFacet>();
#if XBOX
            AddFacet(new SplashScreen(this), null);
#else
            AddFacet(new SplashScreen(this));
#endif

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            //courierNew = Content.LoadFont("Courier New");
            pixel = Content.LoadImage("pixel");
        }
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (facets.Count <= 0) //stack is empty
            {
                Game.Exit();
                return; //evidently Game.Exit() lets things run to completion...hrm.
            }
#if XBOX
            if (Guide.IsVisible) return;
#endif

            // TODO: Add your update code here
            IGameFacet top = facets.Peek();
            top.Update(gameTime);

            if (activeTransition != null)
            {
                activeTransition.Update(gameTime);
                if (activeTransition.Finished)
                {
                    activeTransition.Cleanup();
                    activeTransition = null;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            //stack is empty, it's time to exit
            if (facets.Count <= 0)
                return;

            spriteBatch.Begin();

            if (activeTransition != null)
            {
                activeTransition.Draw(gameTime);
            }
            else
            {
                //draw the active facet
                IGameFacet top = facets.Peek();
                top.Draw(gameTime);
            }

            base.Draw(gameTime);

            spriteBatch.End();
        }
        public void AddFacet(IGameFacet facet)
        {
            AddFacet(facet, null);
        }
        public void AddFacet(IGameFacet facet, FacetTransition transition)
        {
            if (transition != null && facets.Count > 0) 
            {
                if (!transition.IsSetup)
                    transition.Setup(facets.Peek(), facet);
                activeTransition = transition;
            }

            facets.Push(facet);
            facet.Initialize();
        }
        public void ReplaceMeWith(IGameFacet facet)
        {
            ReplaceMeWith(facet, null);
        }
        public void ReplaceMeWith(IGameFacet facet, FacetTransition transition)
        {
            IGameFacet top = facets.Pop();
            facets.Push(facet);
            if (activeTransition == null && transition != null)
            {
                activeTransition = transition;
                if (!activeTransition.IsSetup && facets.Count > 0)
                {
                    activeTransition.Setup(top, facet);
                }
                activeTransition.OnCleanup += (iasr => { top.Cleanup(); });
            }
            facet.Initialize();
        }
        public void PopFacet()
        {
            PopFacet(new FadeTransition(200));
        }
        public void PopFacet(FacetTransition transition)
        {
            //Make sure to only remove the active facet
            IGameFacet top = facets.Pop();
            if (activeTransition == null && transition != null && facets.Count > 0)
            {
                activeTransition = transition;
                if (!activeTransition.IsSetup)
                {
                    activeTransition.Setup(top, facets.Peek());
                }

                //Cleanup the facet when we're done with it
                activeTransition.OnCleanup += (iasr => { top.Cleanup(); });
            }
            else
            {
                top.Cleanup();
            }
            if (facets.Count <= 0)
            {
                Game.Exit();
            }
        }
    }
}