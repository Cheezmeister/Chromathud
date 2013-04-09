using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ChromathudWin.Gameplay
{
    /// <summary>
    /// The game field is where the blocks lie, and where all the action happens
    /// </summary>
    public class Field : MiniGameComponent
    {
        private const int NUM_COLUMNS = 10; //number of columns in the field
        private const int NUM_ROWS = 14 + 1; //number of rows in the field, plus one hidden
        private const int MAX_SELECTION_SIZE = 9; //greatest number of blocks to select
        private const int MAX_SPEED = 30;

        // These two constants control how quickly difficulty ramps up based on score (see updateLevel() )
        private const int BASE_POINTS_PER_LEVEL = 100;
        private const int AUX_POINTS_PER_LEVEL = 5;

        public Point Cursor 
        {
            get { return cursor; }
        }
        private Point cursor = new Point(NUM_COLUMNS / 2, NUM_ROWS);
        public bool UseCursor = false;

        private bool[,] passed = new bool[NUM_ROWS, NUM_COLUMNS];

        private int speedlevel = 1; //internal level
        private int pushcounter = 0; //ticker for pushing blocks upward, in millis
        private long lastsum; //ticks since the last time the player hit the target
        private int riseoffset; //height blocks take on between rows
        private Rectangle area; //entire screen area of the field
        private Rectangle failBlockRect; 
        private Double failBlockSpeed;

        private Texture2D frame; //glowy green outline of the field
        private Texture2D frameBG;
        private Texture2D extension; //bottom bi tof art to cover up blocks
        private Texture2D cursorImage;
        private Texture2D failBlockImage;

        private bool[] shouldfall; //keep track of which columns have falling blocks
        private Random rand;
        private int nextscore; //the score at which we up the level

        /// <summary>
        /// The 2D array of tiles in which blocks may or may not reside
        /// </summary>
        private List<List<Block>> tiles;
        private List<Block> selected;
        private List<Block> fallingblocks;
        private List<Block> dead;
        private bool shouldCheckLow;
        private bool gameover;

        #region Properties
        public Score Score { get; set; }
        public PlayerIndex? Player { get; set; }
        /// <summary>
        /// Are any blocks falling?
        /// </summary>
        public bool BlocksAreFalling
        {
            get 
            { 
                return fallingblocks.Count > 0;
            }
        }
        /// <summary>
        /// Returns the number of rows visible on the field
        /// </summary>
        public static int NumRowsVisible
        {
            get { return NUM_ROWS; }
        }
        public int Target { get; set; }
        /// <summary>
        /// Returns the number of columns visible on the field
        /// </summary>
        public static int NumColumns
        {
            get { return NUM_COLUMNS; }
        }
        /// <summary>
        /// Width in pixels of the game field
        /// </summary>
        public static int Width
        {
            get { return NumColumns * Block.Width; }
        }
        /// <summary>
        /// Height in pixels of the game field
        /// </summary>
        public static int Height
        {
            get { return NumRowsVisible * Block.Height; }
        }
        public long MillisSinceLastSum
        {
            get 
            {
                long now = DateTime.Now.Ticks;
                return now - lastsum; 
            }
        }
        public int SelectLimit
        {
            get { return Math.Min(Level + 2, MAX_SELECTION_SIZE); }
        }
        /// <summary>
        /// Get/set the level. Setting to 0 will start the game in tutorial mode
        /// </summary>
        public int Level
        {
            get { return (speedlevel + 2) / 3; }
            set { speedlevel = Math.Max(value * 3 - 2, 1); }
        }
        public float Speed
        {
            get
            {
                if (speedlevel == 0)
                    return 1 / 2; //prevent division by zero
                return (float)speedlevel / 2F;
            }
        }
        public List<Block> SelectedBlocks
        {
            get { return selected; }
        }
        public int SelectionSize
        {
            get { return selected.Count; }
        }
        public float RisingSpeed
        {
            get 
            {
                if (Player.HasValue && InputManager.GetBoolean(UserCommand.GamePush, Player.Value))
                    return 400;
                return Speed;
            }
        }
        public int RiseOffset
        {
            get { return riseoffset; }
        }
        /// <summary>
        /// The active area of the field, not including its frame
        /// </summary>
        public Rectangle Area
        {
            get { return area; }
            set { area = value; }
        }
        new public GameplayFacet Facet
        {
            get { return (GameplayFacet)((MiniGameComponent)this).Facet; }
        }
        public TopBar TopBar { get; private set; }
        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X coordinate of the top-left corner</param>
        /// <param name="y">Y coordinate of the top-left corner</param>
        public Field(GameFacet f, int x, int y)
            : base(f)
        {
            area = new Rectangle(x, y, Width, Height);

            tiles = new List<List<Block>>();
            selected = new List<Block>();
            fallingblocks = new List<Block>();
            dead = new List<Block>();
            shouldfall = new bool[NUM_COLUMNS];
            pushcounter = 0;
            rand = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
            lastsum = DateTime.Now.Ticks;

            TopBar = new TopBar(this, Area);
            Score = new Score() { Mode = Facet.GameplayMode.ToString()  };
            if (Facet.GameplayMode == GameplayMode.Timed)
                Score.Mode += Facet.ClockTime.Minutes;

            for (int i = 0; i < NUM_ROWS; ++i)
            {
                
                tiles.Add(new List<Block>(NUM_COLUMNS));
                for (int j = 0; j < NUM_COLUMNS; ++j)
                {
                    tiles[i].Add(null);
                }
            }

            //put the first four rows in right away
            for (int i = 0; i < 4; ++i)
                pushBlocks();
            
            // cursor.X = 5;
            // cursor.Y = 13;

            Target = RerollTarget(9); //never gonna give you up
        }

        public void LoadContent()
        {
            string p = (Player == InputManager.ActivePlayer ? "" : 
                Util.IsAlmostEaster() ? "/EE" :
                "/2P");

            frame = Content.LoadImage("TileImages/" + Block.Width + p + "/GameField");
            frameBG = Content.LoadImage("TileImages/" + Block.Width + p + "/GameFieldBG");
            extension = Content.LoadImage("TileImages/" + Block.Width + p + "/GameFieldExtension");
            cursorImage = Content.LoadImage("BlockSelector");
            failBlockImage = Content.LoadImage("TileImages/" + Block.Width + p + "/GameOverBomb");

            TopBar.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            /**
             * Draw the background fill, slightly-translucent black
             */
            Color bgcolor = Util.MakeTranslucent(Color.Black, 150);
  
      
            // If less than two empty rows remain, start pulsing red
            if (closeToTop(2))
            {
                bgcolor = Color.Red;
                bgcolor.R = pulseWarning(gameTime);
            }
            else if (Preferences.GetBoolean("DebugMode"))
            {
                bgcolor = Util.MakeTranslucent(Color.Blue, 50);
                SpriteBatch.DrawTextScaled(area, nextscore.ToString(), 0, 0, Color.White);
            }
            //field frame thickness--the frame is larger than the 
            //active area and needs to be "centered" around it
            int fft = (frame.Width - area.Width) / 2;

            Rectangle framedest = new Rectangle(area.X - fft, area.Y - fft,
                 frame.Width, frame.Height);
            SpriteBatch.Draw(frameBG, framedest, Util.MakeTranslucent(Color.White, 128));

            /**
             * Draw Blocks
             */
            if (Enabled) 
            {
                Rectangle dest = area;
                dest.Inflate(5, 5);
                SpriteBatch.DrawRectangle(dest, bgcolor);

                for (int i = 0; i < NumRowsVisible; ++i)
                {
                    List<Block> l = tiles[i];
                    foreach (Block b in l)
                    {
                        if (b != null)
                            b.Draw(gameTime);
                    }
                }
                foreach (Block b in fallingblocks)
                {
                    b.Draw(gameTime);
                }

                if (UseCursor)
                {
                    Rectangle cursorRect = new Rectangle(area.X, area.Y, Block.Width, Block.Height);
                    cursorRect.X += Cursor.X * Block.Width;
                    cursorRect.Y += Cursor.Y * Block.Height + Block.Height - RiseOffset;
                    Color col = Util.MakeTranslucent(Color.HotPink, 80);
                    SpriteBatch.Draw(cursorImage, cursorRect, Color.White);
                }
            }
            
            if (gameover)
            {
                drawFailAnimation();
            }

            /**
             * Draw the frame on top of the blocks
             */
            if (!Enabled) //grey out fill
            {
                Rectangle dest = area;
                Color grey = Util.MakeTranslucent(Color.Black, 128);
                SpriteBatch.DrawRectangle(dest, grey);
            }
            
            Color tint = Enabled ? Color.White : Color.Gray;
            SpriteBatch.Draw(frame, framedest, tint);

            /**
            * Draw the selection indicator bar
            */
            TopBar.Draw(gameTime);

            //cover up risingblocks on the bottom
            Rectangle botleft = framedest;
            botleft.Y = botleft.Bottom;
            botleft.Height = Block.Height;
            SpriteBatch.Draw(extension, botleft, tint);

            //stylish fade
            botleft.Y = botleft.Bottom;
            botleft.Height = 1;
            do 
            {
                //botleft.Inflate((botleft.Width - framedest.Width) / 5 - 1, 0);
                SpriteBatch.Draw(extension, botleft, tint);
                tint.A -= 5;//= /* (byte)((int))*/ tint.A - 8; // * 31 / 32);
                tint.R -= 5;//= /* (byte)((int))*/ tint.R - 8; // * 24 / 32 - 1);
                tint.G -= 5;//= /* (byte)((int))*/ tint.G - 8; // * 31 / 32);
                tint.B -= 5;//= /* (byte)((int))*/ tint.B - 8; // * 31 / 32);
            }
            while (botleft.Y++ < Facet.FacetManager.GraphicsDevice.Viewport.Height && botleft.Width > 0 && tint.A > 0);

        }
        public override void Update(GameTime gameTime)
        {
            bool targethit = false; 

            if (Player == null)
            {
                for (PlayerIndex p = PlayerIndex.One; p <= PlayerIndex.Four; ++p)
                {
                    if (p == InputManager.ActivePlayer)
                        continue;

                    if (GamePadDelta.ForPlayer(p).HasChanged())
                    {
                        Player = p;
                        break;
                    }
                }
                if (Player == null)
                    return;
            }



            if (!Enabled)
                return;

            if (gameover)
            {
                if (failBlockRect.Width <= 0)
                {
                    failBlockRect = new Rectangle(Area.Left, Area.Top - Area.Width, Area.Width, Area.Width);
                }
                updateFailAnimation();

                return;
            }

            handleInput();

            //update all blocks
            foreach (List<Block> l in tiles)
            {
                foreach (Block b in l)
                {
                    if (b != null)
                        b.Update(gameTime);
                }
            }
            TopBar.Update(gameTime);

            if (BlocksAreFalling)
            {
                //Don't want to mess around with anything below
                //while blocks are falling...bad juju.
                fall();
                return;
            }

            //check for grouping
            clearGroups();


            //add/rise blocks
            pushcounter += (int)((float)gameTime.ElapsedGameTime.Milliseconds * RisingSpeed);

            if (pushcounter > 1000 * 60)
            {
                pushBlocks();
                pushcounter %= (1000 * 60);
            }
            riseoffset = -1 + Block.Height * pushcounter / (1000 * 60);

            if (Facet.GameplayMode == GameplayMode.Timed && Facet.ClockTime <= TimeSpan.Zero)
            {
                GameOver();
            }
            if (GetTotal() == Target)
            {
                HeyTargetWasReached();
                targethit = true;
            }

            int w = Area.Width / 3;
            int y = Area.Y + Area.Height * Layout.NotificationDepth / 100;

            //check for leveling up
            updateLevel();

            //check for a new target
            if (targethit && Preferences.GetBoolean("TargetNotifications"))
            {
                Rectangle location = new Rectangle(Area.X + w, y, w, w);
                Facet.AddComponent(new TargetNotification(Facet, Target, location));
            }

            //prepare to fall
            prepareToFall();

            if (shouldCheckLow && Facet.IsTutorialMode)
            {
                int i = 0;
                foreach (List<Block> l in tiles)
                    foreach (Block b in l)
                        if (b != null)
                            ++i;

                if (i <= NumColumns * 3)
                    Facet.CueTutorialMessage(GameplayFacet.TutorialManager.TutorialEvent.FewBlocks);
                else if (i >= NumColumns * NumRowsVisible / 2)
                    Facet.CueTutorialMessage(GameplayFacet.TutorialManager.TutorialEvent.LotsaBlocks);
            }
        }

        private void drawFailAnimation()
        {
            List<Block> hit = new List<Block>();
            foreach (List<Block> l in tiles)
            {
                Util.Foreach<Block>(l, (b) =>
                {
                    if (b != null && b.Area.Intersects(failBlockRect))
                        hit.Add(b);
                });
                if (hit.Count > 0)
                    Clear(hit);
             }
            SpriteBatch.Draw(failBlockImage, failBlockRect, Color.White);
        }

        private void updateFailAnimation()
        {
            
            failBlockRect.Y += (int)Math.Round(failBlockSpeed);
            failBlockSpeed += 0.9;
            if (failBlockSpeed > 0 && failBlockRect.Bottom > Area.Bottom)
            {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
                failBlockSpeed = -failBlockSpeed * .6;
                if (Math.Abs(failBlockSpeed) < 2)
                {
                    Facet.GameOver(this);
                }
            }
        }

        private void updateLevel()
        {
            int w = Area.Width / 3;
            int y = Area.Y + Area.Height * Layout.NotificationDepth / 100;

            if (Score.Total >= nextscore)
            {
                int oldlevel = Level;
                ++speedlevel;
                nextscore = BASE_POINTS_PER_LEVEL + AUX_POINTS_PER_LEVEL * speedlevel * speedlevel;

                Rectangle location = new Rectangle(Area.X, y, w, w);
                if (Level > oldlevel)
                {
                    Notification not = new Notification(Facet, "limit", location);
                    not.Initialize();
                    Facet.AddComponent(not);
                    Facet.CueTutorialMessage(GameplayFacet.TutorialManager.TutorialEvent.LimitIncreased, SelectLimit);

                }
                else
                {
                    location.X = Area.Right - w;
                    Facet.AddComponent(new Notification(Facet, "speed", location));
                }
            }
        }

        private void handleInput()
        {
            Point temp = Cursor;
            try
            {
                if (InputManager.GetBoolean(UserCommand.GameCursorDown, Player.Value))
                {
                    do
                    {
                        cursor.Y++;
                    } while (tiles[Cursor.Y][Cursor.X] == null);
                    UseCursor = true;
                }
                if (InputManager.GetBoolean(UserCommand.GameCursorUp, Player.Value))
                {
                    do
                    {
                        cursor.Y--;
                    } while (tiles[Cursor.Y][Cursor.X] == null);
                    UseCursor = true;
                }
                if (InputManager.GetBoolean(UserCommand.GameCursorLeft, Player.Value))
                {
                    do
                    {
                        cursor.X--;
                    } while (tiles[Cursor.Y][Cursor.X] == null);
                    UseCursor = true;
                }
                if (InputManager.GetBoolean(UserCommand.GameCursorRight, Player.Value))
                {
                    do
                    {
                        cursor.X++;
                    } while (tiles[Cursor.Y][Cursor.X] == null);
                    UseCursor = true;
                }

            } 
            catch (ArgumentOutOfRangeException e)
            {
                cursor = temp;
            }

            // Deselect last block if hit B some random place on the field
            if (InputManager.GetBoolean(UserCommand.GameDeselect, Player.Value))
            {
                if (selected.Count > 0)
                {
                    Block block = tiles[cursor.Y][cursor.X];
                    if (block != null && !block.Selected)
                    {
                        selected[selected.Count - 1].Selected = false;
                    }
                }

                GamePad.SetVibration(Player.Value, 0, 0);
            }

            if (MouseDelta.GetState().X + MouseDelta.GetState().Y != 0)
                UseCursor = false;

        }

        public void HeyIGotSelected(Block block)
        {
            if (selected.Count < SelectLimit && block.Number != 0)
            {
                selected.Add(block);
                block.Selected = true;
    
                if (selected.Count == SelectLimit && GetTotal() != Target) 
                    Facet.CueTutorialMessage(GameplayFacet.TutorialManager.TutorialEvent.SelectLimitReached);
            }
            else
            {
                SoundManager.PlaySound(SFX.CannotSelect, true);
            }
        }
        public void HeyIGotDeselected(Block block)
        {
            if (selected.Remove(block))
                block.Selected = false;
            if (block.Dead)
            {
                dead.Add(block);
                Facet.CueTutorialMessage(GameplayFacet.TutorialManager.TutorialEvent.DeadBlockCreated);
            }
        }
        public int GetTotal()
        {
            int sum = 0;
            foreach (Block b in selected)
            {
                sum += b.Number;
            }
            return sum;
        }
        public void HeyTargetWasReached()
        {
            int millis = (int)MillisSinceLastSum;
            int count = selected.Count;
            Score.TargetReached(count, SelectLimit, millis);
            selected[count - 1].BarfScore(Score.LastBonus);
            Clear(selected);
            lastsum += millis;
            Target = RerollTarget((SelectLimit - 1) * 9); //Never gonna give you up...
            Facet.CueTutorialMessage(GameplayFacet.TutorialManager.TutorialEvent.TargetChanged, Target);

            Facet.SendMessage("TargetReached", new int[] { (int)Player, count, millis });
        }
        public void DropCrap(int numberOfBlocks, int[] columns)
        {
            int[] stack = new int[NUM_COLUMNS];
            int count = columns.Length;

            for (int i = 0; i < stack.Length; ++i)
                stack[i] = 0;

            if (count <= 0) return;

            for (int i = 0; i < numberOfBlocks; ++i)
            {
                int column = columns[i % count];
                Debug.Assert(column >= 0 && column < NUM_COLUMNS);

                Block block = new Block(this, column, 0, 0);

                int row;
                for (row = tiles.Count - 1; tiles[row][column] != null; --row)
                {
                    if (row <= 0) return;
                }

                block.FallingDestination = row - stack[column];
                ++stack[column];

                if (block.FallingDestination == block.Row)
                    tiles[0][column] = block;
                else
                    fallingblocks.Add(block);
            }
        }
        private void pushBlocks() 
        {
            int rows = 1;

            
            //if there's anything in the top row, game over
            if (tiles[1].Exists(x => (x != null)))
            {
                GameOver();
            }

            foreach (List<Block> l in tiles)
            {
                foreach (Block b in l)
                {
                    if (b != null)
                        b.Row = b.Row - rows; //move all blocks up by the number of rows to push
                }
            }

            //make room at the top
            for (int i = 0; i < rows; ++i)
            {
                tiles.RemoveAt(0);
            }

            //push blocks onto the bottom
            for (int i = 0; i < rows; ++i)
            {
                List<Block> nrow = new List<Block>(NUM_COLUMNS);
                for (int j = 0; j < NUM_COLUMNS; ++j)
                {
                    //create a new block at column j and row i (from the bottom), numbered 1-9
                    Block nblock = new Block(this, j, NUM_ROWS - 1 - i, 0); 
                    nrow.Add(nblock);
                }
                tiles.Add(nrow);
            
                //add new randomized blocks, making sure they don't immediately create any groups
                for (int j = 0; j < NUM_COLUMNS; ++j)
                {
                    do 
                    {
                        tiles[tiles.Count - 1][j].Number = rand.Next(1, Block.NUM_BLOCK_TYPES);
                    } while (new Group(this, tiles.Count - 1, j).Size >= Facet.GroupSize);
                }
                    
            }

            foreach (Block b in fallingblocks)
            {
                //b.Row -= rows;
                b.FallingDestination -= rows;
            }

            //sadly, these aren't linked lists, so have to manually order them (ouch)
            tiles.Sort((row1, row2) => {
                int r1 = 0, r2 = 0;
                foreach (Block b in row1)
                {
                    if (b != null)
                    {
                        r1 = b.Row;
                        break;
                    }
                }
                foreach (Block b in row2)
                {
                    if (b != null)
                    {
                        r2 = b.Row;
                        break;
                    }
                }
                return r1 - r2;
            });
            cursor.Y -= 1;
        }
        public void GameOver()
        {
            this.gameover = true;
            GamePad.SetVibration(Player.GetValueOrDefault(PlayerIndex.One), 0, 0);

        }
        private void fall()
        {
            bool hit = false; //note if any block have landed
            Util.Foreach<Block>(fallingblocks, b =>
            {
                if (b.Falling)
                {
                    b.Fall();
                }
                //has the block landed yet? If so, stick it back on the grid
                if (b.Fallen())
                {
                    hit = true;
                    b.Row = b.FallingDestination;
                    Debug.Assert(tiles[b.Row][b.Column] == null);
                    tiles[b.Row][b.Column] = b;
                    fallingblocks.Remove(b);
                    b.FallingDestination = -1;
                }
            });
           
            if (hit)
                SoundManager.PlaySound(SFX.Clink, false);
        }
        private void clearGroups()
        {
            List<Group> groups = new List<Group>();

            for (int i = 0; i < NUM_ROWS; ++i)
            {
                for (int j = 0; j < NUM_COLUMNS; ++j)
                {
                    if (tiles[i][j] == null)
                        continue;

                    if (passed[i, j])
                        continue;

                    Group g = new Group(this, i, j);
                    g.Report(passed);
                    groups.Add(g);
                }
            }

            foreach (Group g in groups)
            {
                if (g.Size >= Facet.GroupSize)
                {
                    Score.ClearedGroup(g.Size, g.Blocks[1].Number);
                    Rectangle area = g.Blocks[1].Area;
                    area.Inflate(Block.Width * 2, Block.Height * 2);
                    g.Blocks[1].BarfScore(Score.LastBonus);
                    Facet.SendMessage("GroupCleared", (int)Player, g.Size);
                    Clear(g.Blocks);
                }
            }

            //reset marks
            for (int i = 0; i < NUM_ROWS; ++i)
                for (int j = 0; j < NUM_COLUMNS; ++j)
                    passed[i, j] = false;
        }
        private static byte pulseWarning(GameTime gameTime)
        {
            long totalmillis = gameTime.TotalGameTime.Milliseconds;
            return (byte)(85 * Math.Pow(Math.Sin(totalmillis * Math.PI / 1000), 2));
        }
        /// <summary>
        /// Check whether the field is close to filling
        /// </summary>
        /// <param name="threshold">How many rows to allow</param>
        /// <returns></returns>
        private bool closeToTop(int threshold)
        {
            foreach (Block b in tiles[threshold])
                if (b != null) return true;
            return false;
        }
        public void Clear(List<Block> group)
        {
            //see if there are enough blocks adjacent to any dead blocks to clear it
            Util.Foreach<Block>(dead, d =>
            {
                int adjacent = 0;
                foreach (Block b in group)
                {
                    if (d.AdjacentTo(b))
                        ++adjacent;
                }
                if (adjacent >= Facet.DeadClearReq)
                {
                    group.Add(d);
                    dead.Remove(d);
                }
            });

            Util.Foreach<Block>(group, b =>
            {
                foreach (List<Block> l in tiles)
                {
                    int index = l.IndexOf(b);
                    if (index > -1)
                        l[index] = null;
                }
                shouldfall[b.Column] = true;
                if (b.Flavor != Block.FLAVOR_BOMB)
                {
                    b.Aspload();
                    group.Remove(b);
                }
            });

            //clear bombs last
            foreach (Block b in group)
            {
                b.Aspload();
            }
            group.Clear();

            SoundManager.PlaySound(SFX.Clear, true);

            shouldCheckLow = true;
        }

        private void prepareToFall()
        {
            for (int i = 0; i < NUM_COLUMNS; ++i)
            {
                if (!shouldfall[i]) //don't crunch columns we don't have to...
                    continue;

                //working from bottom to top, find the block that will land in each spot
                for (int j = NUM_ROWS - 1, k = NUM_ROWS - 1; j >= 0; --j)
                {
                    if (tiles[j][i] != null)
                    {
                        tiles[j][i].FallingDestination = k--;
                        if (tiles[j][i].Falling)
                        {
                            fallingblocks.Add(tiles[j][i]);
                            tiles[j][i] = null;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Get the blocks adjacent to a given block. 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="shaped">If true, blocks must share an edge to be adjacent. Otherwise a shared corner will suffice.</param>
        /// <returns></returns>
        public List<Block> GetSurrounding(int row, int column, bool shaped)
        {
            List<Block> ret = new List<Block>();

            if (shaped)
            {
                ret.Add(tiles[Math.Max(row - 1, 0)]       [column]);
                ret.Add(tiles[row]                        [Math.Max(column - 1, 0)]);
                ret.Add(tiles[Math.Min(row + 1, NUM_ROWS-1)][column]);
                ret.Add(tiles[row]                        [Math.Min(column + 1, NUM_COLUMNS-1)]);
                //prune null tiles from being "cleared"
                ret.RemoveAll(b => (b == null));
                return ret;
            }

            //if not shaped, return all 8 surrounding blocks
            for (int i = column - 1; i <= column + 1 && i < NUM_COLUMNS; ++i)
            {
                for (int j = row - 1; j <= row + 1 && j < NUM_ROWS; ++j)
                {
                    if (i == column && j == row)
                        continue;
                    if (i < 0 || j < 0)
                        continue;

                    if (tiles[j][i] != null)
                        ret.Add(tiles[j][i]);
                }
            }

            return ret;
        }
        /// <summary>
        /// Generate a new target sum based on current level
        /// </summary>
        /// <returns>New target</returns>
        public int RerollTarget(int max)
        {
            double raw = Util.Random.Next(0, max * max);
            int ret =  (int)Math.Ceiling(Math.Sqrt(raw));
            return ret == 0 ? max : ret;
        }

        private class Group
        {
            private enum Direction
            {
                Left,
                Right,
                Up,
                Down,
                None
            }
            private int num;
            private List<Block> blocks;
            private Field field;

            public List<Block> Blocks
            {
                get { return blocks; }
            }
            public int Size
            {
                get { return blocks.Count; }
            }

            public Group(Field field, int row, int column)
            {
                blocks = new List<Block>();
                this.field = field;
                num = field.tiles[row][column].Number;
                if (num != 0)
                    recursiveFill(row, column, Direction.None);
            }
            public void Report(bool[,] passed)
            {
                foreach (Block b in blocks)
                {
                    passed[b.Row, b.Column] = true;
                }
            }
            private void recursiveFill(int row, int column, Direction from)
            {
                if (column < 0 || column >= NUM_COLUMNS || row < 0 || row >= NUM_ROWS)
                    return;

                Block b = field.tiles[row][column];
                if (b == null)
                    return;

                if (blocks.Contains(b))
                    return;

                if (b.Number != num)
                    return;

                blocks.Add(b);

                if (from != Direction.Left)
                    recursiveFill(row - 1, column, Direction.Right);
                if (from != Direction.Right)
                    recursiveFill(row + 1, column, Direction.Left);
                if (from != Direction.Up)
                    recursiveFill(row, column - 1, Direction.Down);
                if (from != Direction.Down)
                    recursiveFill(row, column + 1, Direction.Up);

            }
        }




    }
}
