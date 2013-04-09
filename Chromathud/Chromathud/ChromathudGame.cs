using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using ChromathudWin.Gameplay;
using EasyStorage;
using System.IO.IsolatedStorage;

namespace ChromathudWin
{
    
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ChromathudGame : Microsoft.Xna.Framework.Game
    {
        public const string Name = "Chromathud";

        public IAsyncSaveDevice SharedSaveDevice { get; private set; }
        public static ChromathudGame Instance { get; private set; }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        public ChromathudGame()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //Adapt to the current graphics hardware
            DetectHardware();

            IsFixedTimeStep = false;
            IsMouseVisible = true;
            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //The InputManager instance automatically updates input
            Components.Add(InputManager.GetInstance(this));

            //The facet manager will do the heavy lifting for event loops
            Components.Add(new FacetManager(this));
#if XBOX
            //Awesome 360 services. Allows for networking stuff.
            Components.Add(new GamerServicesComponent(this));
#endif
            //Let the SoundManager get at the game's ContentManager
            Components.Add(SoundManager.GetInstance(this));

#if WINDOWS && DEBUG
            //Set up for logging
            String path = System.IO.Path.GetTempPath() + "Chromathud\\chromathud.log";
            Trace.Listeners.Add(new TextWriterTraceListener(path));
            Trace.WriteLine("And, we're tracing.");
#endif

            //Set up persistent storage
            EasyStorage.EasyStorageSettings.SetSupportedLanguages(EasyStorage.Language.English);
            SharedSaveDevice = new IsolatedStorageSaveDevice();

            //Load game options, this is ASYNC
            Preferences.LoadAsync(SharedSaveDevice);

            base.Initialize();
        }

#if WINDOWS && DEBUG
        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            Trace.Close();
        }
#endif

        private void DetectHardware()
        {
            //Configure ourselves for the hardware
            DisplayMode dm = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            //DisplayMode dm = graphics.GraphicsDevice.CreationParameters.Adapter.CurrentDisplayMode;           
            if (Util.UseWindowsUI())
            {
                graphics.PreferredBackBufferWidth = 800;
                graphics.PreferredBackBufferHeight = 720;
            }
            else // Xbox UI
            {
                Block.Style = "Nova3/36x36 Graphics/";
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 720;
#if WINDOWS
                graphics.PreferredBackBufferWidth = graphics.PreferredBackBufferWidth * 80 / 100;
                graphics.PreferredBackBufferHeight = graphics.PreferredBackBufferHeight * 80 / 100;
#endif
            }
        }
        protected override void  BeginRun()
        {
            base.BeginRun();
        }
        protected override void EndRun()
        {
            Preferences.SaveAsync(SharedSaveDevice);
            SoundManager.Cleanup();
            base.EndRun();
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Util.Pixel = Content.LoadImage("pixel");
            Util.GlobalFont = Content.LoadFont("chroma");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (InputManager.GetBoolean(UserCommand.ExitProgram))
                this.Exit();
#if XBOX && DEBUG
            if (InputManager.GetBoolean(UserCommand.ToggleTrialMode))
                Guide.SimulateTrialMode = !Guide.SimulateTrialMode;
#endif
            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);

            spriteBatch.Begin();
            if (Preferences.GetBoolean("DebugMode"))
            {
                //print FPS in the top left corner, avoiding divide by zero
                float fps = TimeSpan.TicksPerSecond / (1 + gameTime.ElapsedGameTime.Ticks);
                Rectangle rect = new Rectangle(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y, 100, 20);
                spriteBatch.DrawTextScaled(rect, "FPS: " + fps.ToString(), -1, -1, Color.Blue);
                rect.Y = rect.Bottom;
                if (Guide.SimulateTrialMode)
                    spriteBatch.DrawTextScaled(rect, "Trial Mode", -1, -1, Color.Green);
            }
            spriteBatch.End();
        }

        private void SetDefaultPref(string pref, Object value)
        {
            Preferences.SetDefault(pref, value);
        }
    }
}
