using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ChromathudWin.Gameplay
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Instructions : GameFacet
    {
        //need to know how many screens there are total
        private const int NUM_SCREENS = 10;

        //screens named "Instructions_1.png", etc
#if XBOX || EMULATE_XBOX
        private const string INSTRUCTION_PREFIX = "x";
#else
        private const string INSTRUCTION_PREFIX = "Instructions_";
#endif

        //folder name - change if needed
        private const string INSTRUCTION_FOLDER = "instros/";

        private Texture2D bg;
        private List<Texture2D> screens;
        private int current;

        public Instructions(FacetManager fm)
            : base(fm)
        {
            screens = new List<Texture2D>();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
            Mouse.SetPosition(FacetManager.GraphicsDevice.Viewport.Width / 2, FacetManager.GraphicsDevice.Viewport.Height - 10);
        }

        public override void LoadContent()
        {
            bg = Content.LoadImage("TileImages/BGTile");
            for (int i = 0; i < NUM_SCREENS; ++i)
            {
                Texture2D loaded = Content.LoadImage(
                    INSTRUCTION_FOLDER + INSTRUCTION_PREFIX + i.ToString()
                    );
                screens.Add(loaded);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void UnloadContent()
        {
            
        }

        public override void Draw(GameTime gameTime)
        {
            //draw the tiled background
            Vector2 location = new Vector2();
            for (int x = 0; x < FacetManager.GraphicsDevice.Viewport.Width; x += bg.Width)
            {
                location.X = x;
                for (int y = 0; y < FacetManager.GraphicsDevice.Viewport.Height; y += bg.Height)
                {
                    location.Y = y;
                    SpriteBatch.Draw(bg, location, Color.White);
                }
            }
            Rectangle tsa = FacetManager.GraphicsDevice.Viewport.TitleSafeArea;

            FacetManager.SpriteBatch.Draw(screens[current], screens[current].FitIn(tsa, 0, 0), Color.White);
        }
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (InputManager.GetBoolean(UserCommand.SceneSkip))
                Exit();

            if (InputManager.GetBoolean(UserCommand.SceneBack))
            {
                if (current == 0) 
                    Exit();
                else
                    --current;
            }
            if (InputManager.GetBoolean(UserCommand.SceneNext))
            {
                if (current >= screens.Count - 1)
                    Exit();
                else
                    ++current;
            }
            base.Update(gameTime);
        }
    }
}