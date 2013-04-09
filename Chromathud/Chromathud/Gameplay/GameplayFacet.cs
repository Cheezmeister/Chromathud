using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace ChromathudWin.Gameplay
{

    public enum GameplayMode
    {
        Endless,
        Timed
    }

    /// <summary>
    /// The meat and potatoes of the game, pending a less-sucky name
    /// </summary>
    public class GameplayFacet : GameFacet
    {
    
        private bool tutorialMode; //tutorial mode flag
        private bool singlePlayer = true; //single player flag
        private float yBGOffset;
        private float xBGOffset;

        private Texture2D background;
        private Field SinglePlayerField
        {
            get
            {
                return fields[0];
            }
            set
            {
                fields[0] = value;
            }
        }
        private List<Field> fields;

        private WickedLibrary.Graphics.ParticleSystem<WickedLibrary.Graphics.ParticleSets.PhysicsParticle> particleSys;
        
        private List<MiniGameComponent> components;
        public TutorialManager TutManager { get; private set; }

        private Queue<KeyValuePair<string, object>> messageQueue;

        #region Properties
        public WickedLibrary.Graphics.ParticleSystem<WickedLibrary.Graphics.ParticleSets.PhysicsParticle> ParticleSystem
        {
            get { return particleSys; }
            set { particleSys = value; }
        }
        public GameplayMode GameplayMode { get; set; }
        public int MinuteLimit { get; set; }

        public TimeSpan GameplayTime { get; private set; }

        /// <summary>
        /// How much time is on the clock
        /// </summary>
        public TimeSpan ClockTime 
        {
            get
            {
                if (GameplayMode == GameplayMode.Timed)
                    return Util.Max(new TimeSpan(0, MinuteLimit, 0) - GameplayTime, TimeSpan.Zero);
                else
                    return GameplayTime;
            }
        }
        
        /// <summary>
        /// The game field
        /// </summary>
        //public Field Field { get; private set; }

        /// <summary>
        /// Is the interactive tutorial active? 
        /// </summary>
        public bool IsTutorialMode
        {
            get { return tutorialMode && singlePlayer; }
            set { tutorialMode = value; }
        }
        /// <summary>
        /// Get/set single player mode. This is the default and two players is assumed otherwise.
        /// </summary>
        public bool SinglePlayer
        {
            get { return singlePlayer; }
            set { singlePlayer = value; }
        }
         /// <summary>
        /// How many adjacent blocks are needed to remove a dead block
        /// </summary>
        public int DeadClearReq
        {
            get { return 2; }
        }
        /// <summary>
        /// How many adjacent blocks are needed to group clear
        /// </summary>
        public int GroupSize
        {
            get { return 4; }
        }

        /// <summary>
        /// The initial gameplay level can be set prior to starting the game
        /// </summary>
        public int Level
        {
            set 
            {
                if (value == 0) 
                    throw new Exception("Don't do that!");
                this.level = value;
                foreach (MiniGameComponent c in components)
                {
                    if (c is Field)
                        ((Field)c).Level = level;
                }
            }
        }
        private int level = 1;
        #endregion

        public GameplayFacet(FacetManager fm)
            : base(fm)
        {
            components = new List<MiniGameComponent>();
            messageQueue = new Queue<KeyValuePair<string, object>>();
        }
        public override void Initialize()
        {
            base.Initialize();
            GameplayTime = TimeSpan.Zero;
            
            fields = new List<Field>();

            particleSys = new WickedLibrary.Graphics.ParticleSystem<WickedLibrary.Graphics.ParticleSets.PhysicsParticle>(1000);

            layoutComponents();


        }

        private void layoutComponents()
        {
            Timer timer = new Timer(this);
            Field field;

            Point scenter;
            components.Add(timer);
            Rectangle tsa = FacetManager.GraphicsDevice.Viewport.TitleSafeArea;

            if (SinglePlayer)
            {
                int x = tsa.Center.X - Field.Width / 2;
                int y = tsa.Y;
                field = new Field(this, x, y);
                field.Player = InputManager.ActivePlayer;
                field.LoadContent();
                field.Level = level;
                components.Add(field);
                fields.Add(field);

                scenter = new Point(
                    (field.Area.Right + tsa.Right) / 2,
                    (tsa.Bottom - Layout.BottomBuffer - Layout.StatusOneHeight / 2)
                    );

                StatusDisplay status = new StatusDisplay(field, scenter);
                components.Add(status);
                Rectangle boxarea = new Rectangle(status.Area.X + Block.Width / 2, status.Area.Y - Block.Height * 2, status.Area.Width, Block.Height * 2);
                SelectionBox box = new SelectionBox(field, boxarea);
                components.Add(box);

                timer.Center = new Point(
                    (field.Area.Left + tsa.Left) / 2,
                    (tsa.Top + status.Area.Top) / 2);
                TutManager = new TutorialManager(this, SinglePlayerField.Area);
                CueTutorialMessage(TutorialManager.TutorialEvent.Intro, field.Target);
            }
            else
            {
                // 1 Player
                int x = tsa.X;
                int y = tsa.Y;
                field = new Field(this, x, y);
                field.Player = InputManager.ActivePlayer;
                field.LoadContent();
                components.Add(field);
                fields.Add(field);

                scenter = new Point(tsa.Center.X, tsa.Top + Layout.StatusOneHeight / 2);
                StatusDisplay status = new StatusDisplay(field, scenter);
                components.Add(status);

                Rectangle boxarea = new Rectangle(field.Area.Right, field.Area.Y, Block.Width * 9, Block.Height);
                boxarea.X += (field.Area.Width - boxarea.Width) / 2;
                SelectionBox box = new SelectionBox(field, boxarea);
                components.Add(box);

                // 2 Player
                x = tsa.Right - Field.Width;
                y = y;

                field = new Field(this, x, y);
                field.Player = null;
                field.LoadContent();
                components.Add(field);
                fields.Add(field);


                scenter = new Point(tsa.Center.X, tsa.Bottom - Layout.StatusTwoHeight / 2);
                status = new StatusDisplay(field, scenter);
                components.Add(status);

                boxarea = new Rectangle(field.Area.X - Block.Width * 7, field.Area.Bottom, Block.Width * 9, Block.Height);
                boxarea.X += (field.Area.Width - boxarea.Width) / 2;
                box = new SelectionBox(field, boxarea);
                components.Add(box);

                timer.Center = tsa.Center;
            }
        }
        public override void LoadContent()
        {
            Block.LoadContent(Content);
            background = Content.LoadImage("TileImages/BGTile" + (SinglePlayer ? "" : "2"));
            SoundManager.PlayBGM((int)this.GameplayMode + (this.SinglePlayer ? 0 : 2));
        }
        public override void UnloadContent()
        {
            SoundManager.StopBGM();
        }
        /// <summary>
        /// Draw background and other decorative stuff, then all game components
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            
            //draw the tiled background
            Vector2 location = new Vector2();
            xBGOffset += ((float)gameTime.ElapsedGameTime.Milliseconds *
                CumulativeTime.Minutes / 100);
            yBGOffset += Math.Abs((float)gameTime.ElapsedGameTime.Milliseconds *
                ((float)Math.Sin(CumulativeTime.Minutes)) / 100);

            xBGOffset %= background.Width;
            yBGOffset %= background.Height;
            
            for (int x = -background.Width; x < FacetManager.GraphicsDevice.Viewport.Width + background.Height; x += background.Width)
            {
                location.X = x - xBGOffset; 
                for (int y = -background.Height; y < FacetManager.GraphicsDevice.Viewport.Height + background.Height; y += background.Height)
                {
                    location.Y = y - yBGOffset;
                    SpriteBatch.Draw(background, location, Color.White);
                }
            }

            Color color = Color.Black;


            //draw subcomponents
            Util.Foreach(components, item =>
              { item.Draw(gameTime); }
            );

            if (Preferences.GetBoolean("DebugMode"))
            {
                Rectangle rect = new Rectangle(
                    (int)InputManager.GetFloat(UserCommand.GameCursorX),
                    (int)InputManager.GetFloat(UserCommand.GameCursorY),
                    16,
                    16
                );
                SpriteBatch.DrawRectangle(rect, Color.Blue);
            }

            //draw particles
            ParticleSystem.Draw(FacetManager.GraphicsDevice, SpriteBatch);

            if (playerIsDisconnected())
            {
                SpriteBatch.DrawRectangle(FacetManager.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            }
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            dispatchMessages();

            if (playerIsDisconnected()) return;

            base.Update(gameTime);
            GameplayTime += gameTime.ElapsedGameTime;

            if (InputManager.GetBoolean(UserCommand.GameQuit))
            {
                Exit();
                return; 
                //exit again with more free swag
            }

            handleInput();
  
            //update subcomponents
            Util.Foreach(components, item =>
            {
                item.Update(gameTime);
                if (!item.Alive)
                    components.Remove(item);
            });

            if (IsTutorialMode)
            {
                TutManager.Update(gameTime);
            }
            ParticleSystem.Update(gameTime);
        }

        private bool playerIsDisconnected()
        {
            bool ret = false;
#if XBOX
            foreach (Field f in fields)
            {
                ret |= (f.Player.HasValue && !GamePad.GetState(f.Player.Value).IsConnected);
            }
#endif
            return ret;
        }

        /// <summary>
        /// Poll and take care of user input
        /// </summary>
        private void handleInput()
        {
            if (InputManager.GetBoolean(UserCommand.GamePause))
            {
                FacetManager.AddFacet(new PauseScreen(FacetManager), new FadeTransition(200));
            }
        }

        public void AddComponent(MiniGameComponent component)
        {
            component.Initialize();
         //   component.LoadContent();
            components.Add(component);
        }

        public void CueTutorialMessage(TutorialManager.TutorialEvent evt)
        {
            CueTutorialMessage(evt, null);
        }
        public void CueTutorialMessage(TutorialManager.TutorialEvent evt, object arg0)
        {
            CueTutorialMessage(evt, arg0, null);
        }
        public void CueTutorialMessage(TutorialManager.TutorialEvent evt, object arg0, object arg1)
        {
            if (TutManager != null)
                TutManager.NotifyReady(evt, arg0, arg1);
        }




        ///////////////////////////////////////////////////////////////////////
        // TutorialManager
        ///////////////////////////////////////////////////////////////////////




        /// <summary>
        /// Handle the interactive tutorial, if enabled
        /// </summary>
        public class TutorialManager : MiniGameComponent
        {
            private int ticker;
            private bool finished = false;
            private bool totallyFinished = false;

            private TutorialMessage nextmessage;

            private Dictionary<TutorialEvent, bool> done;

            /// <summary>
            /// If ticker is set to this value, it is disabled
            /// </summary>
            private const int NOTICKER = int.MinValue;

            public enum TutorialEvent
            {
                Intro,
                LotsaBlocks,
                SelectLimitReached,
                DeadBlockCreated,
                TargetChanged,
                LimitIncreased,
                FewBlocks,
                Done
            }

            public Rectangle Area { get; set; }
            new public GameplayFacet Facet
            {
                get { return (GameplayFacet)((MiniGameComponent)this).Facet; }
            }

            private int delayMillis = 5000;

            public int DelayMillis
            {
                get { return delayMillis; }
                set { delayMillis = value; }
            }

            public TutorialManager(GameplayFacet gf, Rectangle textArea)
                : base(gf)
            {
                Area = textArea;
                done = new Dictionary<TutorialEvent, bool>();
            }

            public override void Draw(GameTime gameTime)
            {

            }

            public override void Update(GameTime gameTime)
            {
                if (!Facet.IsTutorialMode)
                    return;

                ticker -= gameTime.ElapsedGameTime.Milliseconds;
                if (ticker < 0)
                {
                    Facet.FacetManager.AddFacet(nextmessage, new FadeTransition(400));
                    ticker = NOTICKER;
                    if (finished && !totallyFinished)
                    {
                        string messageStr = Chromathud.Strings.TutorialCompleted;
                        nextmessage = new TutorialMessage(Facet, messageStr, MessageDest);
                        ticker = 4000;
                        totallyFinished = true;
                    }
                }


            }
            public void NotifyReady(TutorialEvent happened, object arg0, object arg1)
            {
                const int stdFuse = 400; //usual time to delay before popping up a tutorial message
                string messageStr = "";
                Rectangle dest = MessageDest;
                Rectangle imageDest = dest;
                imageDest.Y += imageDest.Height;

                // Don't show tutorial messages more than once
                if (done.Keys.Contains(happened) && done[happened])
                    return;

                done.Add(happened, true);

                if (done.Count >= 7) // TODO don't hardcode # events
                    finished = true;

                string pushing, selectLimit, intro;
                if (Util.UseXboxUI())
                {
                    pushing = Chromathud.Strings.TutorialPushingXbox;
                    selectLimit = Chromathud.Strings.TutorialSelectLimitXbox;
                    intro = Chromathud.Strings.TutorialIntroXbox;
                }
                else
                {
                    pushing = Chromathud.Strings.TutorialPushingPC;
                    selectLimit = Chromathud.Strings.TutorialSelectLimitPC;
                    intro = Chromathud.Strings.TutorialIntroPC;
                }
                switch (happened)
                {
                    
                    case TutorialEvent.Intro:
                        messageStr = String.Format(Chromathud.Strings.TutorialIntro, (int)arg0, intro);
                        nextmessage = new TutorialMessage(Facet, messageStr, dest);
                        ticker = 500;
                        break;
                    case TutorialEvent.TargetChanged:
                        messageStr = String.Format(Chromathud.Strings.TutorialTargetChanged, (int)arg0);
                        nextmessage = new TutorialMessage(Facet, messageStr, dest);
                        ticker = stdFuse;
                        break;
                    case TutorialEvent.LotsaBlocks:
                        messageStr = String.Format(Chromathud.Strings.TutorialHowToLose);
                        nextmessage = new TutorialMessage(Facet, messageStr, dest);
                        ticker = stdFuse;
                        break;
                    case TutorialEvent.FewBlocks:
                        messageStr = String.Format(Chromathud.Strings.TutorialPushing, pushing);
                        nextmessage = new TutorialMessage(Facet, messageStr, dest);
                        if (Util.UseWindowsUI())
                        {
                            nextmessage.Image = Content.LoadImage("instros/spacebar");
                            nextmessage.ImageArea = imageDest;
                        }
                        ticker = stdFuse;
                        break;
                    case TutorialEvent.DeadBlockCreated:
                        messageStr = String.Format(Chromathud.Strings.TutorialDeadBlocks);
                        nextmessage = new TutorialMessage(Facet, messageStr, dest);
                        ticker = stdFuse;
                        break;
                    case TutorialEvent.SelectLimitReached:
                        messageStr = String.Format(Chromathud.Strings.TutorialSelectLimitReached, selectLimit);
                        nextmessage = new TutorialMessage(Facet, messageStr, dest);
                        ticker = 600;
                        break;
                    case TutorialEvent.LimitIncreased:
                        messageStr = String.Format(Chromathud.Strings.TutorialLimitIncreased, arg0);
                        nextmessage = new TutorialMessage(Facet, messageStr, dest);
                        ticker = stdFuse;
                        break;
                }
            }

            private Rectangle MessageDest
            {
                get 
                {
                    Rectangle dest = Area;
                    dest.Height /= 2;
                    dest.Y += dest.Height / 5;

                    return dest;                
                }
            }

            private class TutorialMessage : GameFacet
            {
                private string text;
                private int minTime = 1; // 1 second minimum to avoid accidental quits
                private GameFacet parent;
                public Rectangle Area { get; set; }
                public Texture2D Image { get; set; }
                public Rectangle ImageArea { get; set; }

                /// <summary>
                /// Construct a tutorial message to introduce a single aspect of gameplay
                /// </summary>
                /// <param name="parent"></param>
                /// <param name="message"></param>
                /// <param name="delayMS"></param>
                public TutorialMessage(GameFacet parent, string message, Rectangle area)
                    : base(parent.FacetManager)
                {
                    this.parent = parent;
                    text = Util.SubstituteButtonChars(message);
                    //text = Util.WrapToAspect(text, (area.Width / area.Height));
                    text = Util.WrapToLength(text, 24); // HACK
                    this.Area = area;
                }
                public override void LoadContent()
                {

                }
                public override void UnloadContent()
                {

                }
                public override void Update(GameTime gameTime)
                {
                    base.Update(gameTime);
                    if (this.CumulativeTime.TotalSeconds > minTime)
                    if (InputManager.GetBoolean(UserCommand.SceneNext) ||
                        InputManager.GetBoolean(UserCommand.SceneSkip))
                    {
                        Exit();
                    }
                }
                public override void Draw(GameTime gameTime)
                {
                    //draw the parent facet (namely the game screen) washed out
                    parent.Draw(gameTime);
                    Color wash = Color.Black.MakeTranslucent(128);
                    SpriteBatch.DrawRectangle(Util.GetRect(FacetManager.GraphicsDevice.Viewport), wash);

                    Rectangle tsa = FacetManager.GraphicsDevice.Viewport.TitleSafeArea;

                    //SpriteBatch.DrawRectangle(Area, Color.Red.MakeTranslucent(128));

                    SpriteFont font = Content.LoadFont("Courier New");
                    FacetManager.SpriteBatch.DrawTextScaled(Area, text, 0, -1, Color.White);

                    if (Image != null && ImageArea != null)
                    {
                        SpriteBatch.Draw(Image, Image.FitIn(ImageArea,0, -1), Color.White);
                    }
                }

            }

            public override void Initialize()
            {

            }
        }




        public void GameOver(Field field)
        {
            if (SinglePlayer)
            {
                SoundManager.StopBGM();
                FacetManager.ReplaceMeWith(
                    new GameOverScreen(SinglePlayerField),
                    new FadeTransition(2000)
                    );

            }
            else
            {
                Field loser = field;
                Field winner = fields[loser.Player == InputManager.ActivePlayer ? 1 : 0];

                FacetManager.ReplaceMeWith(
                    new MultiplayerOverScreen(FacetManager, winner, loser),
                    new FadeTransition(2000)
                    );
            }
            foreach (Field f in fields)
            {
                GamePad.SetVibration(f.Player.GetValueOrDefault(PlayerIndex.One), 0, 0);
            }

        }

        private void dispatchMessages()
        {
            int i = messageQueue.Count;
            while (i-- > 0)
            {
                var message = messageQueue.Dequeue();
                handleMessage(message.Key, message.Value);
            }
        }
        private void handleMessage(string message, object arg)
        {
            bool alreadyDropped = false;
            try
            {
                object[] argArray = (object[])arg;
                switch (message)
                {
                    case "TargetReached":
                        if (SinglePlayer) break;

                        //handleTargetReached((int)argArray[0], (int)argArray[1], (int)argArray[2]);
                        break;
                    case "GroupCleared":
                        if (SinglePlayer) break;
                        if (alreadyDropped)
                        {
                            SendMessage(message, argArray);
                            break;
                        }

                        int player = (int)argArray[0];
                        int count = (int)argArray[1] - 3;
                        player = handleGroupCleared(player, count);
                        alreadyDropped = true;
                        break;
                }
            }
            catch (InvalidCastException e)
            {
                //Trace.WriteLine("Invalid args to message " + message + ", ignoring.");
            }
        }

        private int handleGroupCleared(int player, int count)
        {
            //if (count <= 0) break;

            // Drop on the other player's field
            int otherplayer = (player == (int)fields[0].Player) ? 1 : 0;

            int[] columns = new int[count];
            for (int i = 0; i < count; ++i)
                columns[i] = Util.Random.Next(10);

            fields[otherplayer].DropCrap(count, columns);
            return otherplayer;
        }

        private void handleTargetReached(int player, int count, int millis)
        {
            
            if (count <= 0) return;

            int[] columns = new int[count];
            for (int i = 0; i < count; ++i)
                columns[i] = Util.Random.Next(10);

            // Drop on the other player's field
            player = player == 1 ? 0 : 1;

            fields[player].DropCrap(count, columns);
        }

        public void SendMessage(string message, params object[] args)
        {
            messageQueue.Enqueue(new KeyValuePair<string, object>(message, args));
        }
    }
}
