using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using ChromathudWin;
using WickedLibrary.Graphics.Hax;
using Microsoft.Xna.Framework.Input;

namespace ChromathudWin.Gameplay
{
    public class Block : MiniGameComponent
    {
        /// <summary>
        /// Width of each block in pixels
        /// </summary>
        public /* static */ const int Width = Layout.BlockWidth;
        /// <summary>
        /// Height of each block
        /// </summary>
        public /* static */ const int Height = Width;
        /// <summary>
        /// Number of block types, 1-9 plus 'dead' blocks
        /// </summary>
        public const int NUM_BLOCK_TYPES = 10;

        /// <summary>
        /// A block's flavor is its behaviour. Bomb blocks destroy adjacent 
        /// blocks when they're cleared, and clock blocks slow down the rising
        /// speed temporarily
        /// </summary>
        public const int NUM_FLAVORS = 2;
        public const int FLAVOR_NORMAL = 0;
        public const int FLAVOR_BOMB = 1;
        
        //gauge speed at which blocks fall for aesthetics
        private const float FALLING_SPEED = 0.4F;

        //delay in millis for blocks destroyed by bomb blocks
        private const int ASPLOSION_DELAY = 100;

        //the number used by dead blocks
        private const int DEAD = 0;

        private const int NUM_IMAGES = NUM_BLOCK_TYPES;
        private static Texture2D bombHighlight;
        private static List<Texture2D>[] sprites;
        private static List<Texture2D>[] spritesSelected;
        private static List<Texture2D>[] spritesHovered;
        private static Color[] colors = new Color[NUM_BLOCK_TYPES];
     
        //instance vars
        private int type; //1-9, 0 signifies a dead block
        private int flavor; //block behaviour
        private int row, column; //logical coordinates
        private int fallingDestination;
        private int fallingOffset;
        private int fallingSpeed;
        private bool hovered; //mouseover flag
        private bool selected; //selection flag
        private Rectangle _area;

        
        private static string style = "Nova3/";
        public static string Style 
        {
            get
            {
                return style;
            }
            set
            {
                style = value;
            }
        }

        protected Field field;
        private bool wasHovered;

        #region Properties
        private int FallingOffset
        {
            get { return (int)(fallingOffset * FALLING_SPEED); }
            set { fallingOffset = value; }
        }
        public int FallingDestination
        {
            get { if (fallingDestination > 0) return fallingDestination; return Row; }
            set { if (value != row) fallingDestination = value; }
        }
        public int Row
        {
            get { return row; }
            set 
            { 
                _area.Y -= (row - value) * Height; 
                row = value; 
                fallingOffset = fallingSpeed = 0;
            }
        }
        public int Column
        {
            get { return column; }
            set { column = value; _area.X = column * Width; }
        }
        public int Number
        {
            get { return type; }
            set
            {
                type = value;
            }
        }
        public Color Color
        {
            get
            {
                return colors[Number];
            }
        }
        public int Flavor
        {
            get { return flavor; }
        }
        public bool Falling
        {
            get { return fallingDestination > row; }
        }
        public bool Dead
        {
            get { return type == Block.DEAD; }
        }
        public bool Selected
        {
            get { return selected; }
            set
            {
                if (value == selected)
                    return;
                if (value == false)
                {
                    SoundManager.PlaySound(SFX.Deselect, false);
                    type = 0;
                    field.HeyIGotDeselected(this);
                }
                else
                {
                    SoundManager.PlaySound(SFX.Select, false);
                }
                selected = value;
            }
        }
        public Rectangle Area
        {
            get 
            { 
                Rectangle temp = _area; 
                temp.Y -= field.RiseOffset - FallingOffset; 
                return temp; 
            }
        }
        //public bool TimeToAspload
        //{
        //    get
        //    {
        //        return markedForAsplosion && asplosionTimer <= 0;
        //    }
        //}
        #endregion
        
        public static void LoadContent(ContentManager content)
        {
            //load just one copy of each sprite
            sprites = new List<Texture2D>[NUM_FLAVORS];
            spritesSelected = new List<Texture2D>[NUM_FLAVORS];
            spritesHovered = new List<Texture2D>[NUM_FLAVORS];
            for (int i = 0; i < NUM_FLAVORS; ++i)
            {
                sprites[i]         = new List<Texture2D>(NUM_IMAGES);
                spritesSelected[i] = new List<Texture2D>(NUM_IMAGES);
                spritesHovered[i]  = new List<Texture2D>(NUM_IMAGES);
            }


            //image naming conventions for different flavors
            string[] flavors = new string[NUM_FLAVORS];
            flavors[FLAVOR_NORMAL] = "";
            flavors[FLAVOR_BOMB] = "Bomb";

            //Load the dead block sprite, using it for all dead blocks regardless of flavor
            for (int i = 0; i < NUM_FLAVORS; ++i)
            {
                sprites[i].Add(content.LoadImage(style + "Dead"));
                spritesSelected[i].Add(sprites[0][0]);
                spritesHovered[i].Add(sprites[0][0]);
            }
            for (int i = 1; i < NUM_IMAGES; ++i)
            {
                for (int j = 0; j < NUM_FLAVORS; ++j)
                {
                    sprites[j].Add(content.LoadImage(style + i.ToString() + flavors[j]));
                    spritesSelected[j].Add(content.LoadImage(style + i.ToString() + flavors[j] + "selected2"));
                    spritesHovered[j].Add(content.LoadImage(style + i.ToString() + flavors[j] + "selected"));
                }
            }

            bombHighlight = content.LoadImage(style + "BombHighlightS");

            //colors that roughly correspond to those for each image
            colors[0] = Color.Gray;
            colors[1] = Color.Red;
            colors[2] = Color.HotPink;
            colors[3] = Color.Purple;
            colors[4] = Color.Blue;
            colors[5] = Color.Cyan;
            colors[6] = Color.Green;
            colors[7] = Color.GreenYellow;
            colors[8] = Color.Yellow;
            colors[9] = Color.Orange;
        }
        public static Texture2D getSprite(int type, int flavor, bool selected, bool hovered)
        {
            if (hovered)
                return spritesHovered[flavor][type];
            if (selected)
                return spritesSelected[flavor][type];
            return sprites[flavor][type];
        }

        public Block(Field field, int column, int row, int type)
            : base(field.Facet)
        {
            this.column = column;
            this.row = row;
            this.type = type;
            this.field = field;
            _area = new Rectangle(field.Area.X + column  * Width,
                                 field.Area.Y + (row+1) * Height,
                                 Width, Height);

            flavor = FLAVOR_NORMAL;
            if (Util.Random.Next(42) < 1)
                flavor = FLAVOR_BOMB;
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Rectangle dest = Area;
            Texture2D sprite = getSprite(type, flavor, selected, hovered);

            //highlight the area the explosion will affect
            if ((hovered || selected) && flavor == FLAVOR_BOMB && !Dead)
            {
                drawBombHighlight(dest);
            }

            SpriteBatch.Draw(sprite, dest, Color.White);
            
        }
        public void DrawCopyAt(GameTime gameTime, Rectangle area)
        {
            Color tint = Color.White;
            Texture2D sprite = getSprite(type, flavor, false, false);
            SpriteBatch.Draw(sprite, area, tint);
        }

        private void drawBombHighlight(Rectangle dest)
        {
            dest.Width *= 3;
            dest.Height *= 3;
            dest.X -= Width;
            dest.Y -= Height;

            SpriteBatch.Draw(bombHighlight, dest, colors[Number]);
        }
        public override void Update(GameTime gameTime)
        {
            //where is the cursor?
            Point cursor = new Point((int)InputManager.GetFloat(UserCommand.GameCursorX),
                                     (int)InputManager.GetFloat(UserCommand.GameCursorY));

            //is it over this block?
            if (field.UseCursor)
                hovered = (field.Cursor.X == Column && field.Cursor.Y == Row);
            else
                hovered = Area.Contains(cursor);

            if (hovered)
            {
                if (!wasHovered)
                {
                    SoundManager.PlaySound(SFX.Tick, true);
                }
                if (field.Player.HasValue && InputManager.GetBoolean(UserCommand.GameSelect, field.Player.Value))
                {
                    if (!selected)
                        field.HeyIGotSelected(this);

                    GamePad.SetVibration(field.Player.Value, 0, 0);

                }
                else if (field.Player.HasValue && InputManager.GetBoolean(UserCommand.GameDeselect, field.Player.Value))
                {
                    Selected = false;
                }
            }

            wasHovered = hovered;
        }

        /// <summary>
        /// Particles?
        /// </summary>
        public void Aspload()
        {
            int particliness = 20;
            Texture2D sprite = Content.LoadImage("pixel");
            WickedLibrary.Graphics.Hax.TextureData texdata = 
                new WickedLibrary.Graphics.Hax.TextureData() 
                { Texture = sprite };

            for (int i = 0; i < particliness; ++i)
            {
                Vector4 col = colors[Number].ToVector4();
                ((GameplayFacet)(Facet)).ParticleSystem.AddParticle(
                    new AwesomeParticle()
                    {
                        Position = new Vector2(Area.Center.X, Area.Center.Y),
                        Texture = texdata,
                        MaxAge = .6F,
                        InitScale = 15.0F,
                        InitColor = col,
                        EndColor = new Vector4(0, 0, 0, 0),
                        Velocity = new Vector2(Util.Random.Next(400) - 200, -Util.Random.Next(400) + 200),
                        Gravity = new Vector2(0, 0)
                    }
                );
            }
            if (flavor == FLAVOR_BOMB)
            {
                List<Block> l = field.GetSurrounding(row, column, false);
                field.Clear(l);
                SoundManager.PlaySound(SFX.Thump, false);
                if (field.Player.HasValue)
                {
                    if (GamePad.SetVibration(field.Player.Value, 0.8f, 0.8f))
                    {
                        System.Threading.Timer t = new System.Threading.Timer(o => {
                            GamePad.SetVibration(field.Player.Value, 0, 0);
                        }, null, 300, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Fall one frame's worth, accelerating
        /// </summary>
        public void Fall()
        {
            fallingOffset += ++fallingSpeed;
        }

        /// <summary>
        /// Has the block landed?
        /// </summary>
        /// <returns></returns>
        public bool Fallen()
        {
            return FallingOffset >= (FallingDestination - row) * Height;
        }
        
        /// <summary>
        /// Is this block adjacent to (share a side with) another?
        /// </summary>
        /// <param name="b">The other block</param>
        /// <returns></returns>
        public bool AdjacentTo(Block b)
        {
            return (Math.Abs(b.row - row) == 1 && b.column == column) ||
                (Math.Abs(b.column - column) == 1 && b.row == row);
        }

        private class AwesomeParticle : WickedLibrary.Graphics.StdParticle
        {
            public Vector2 Gravity { get; set; }
            public AwesomeParticle()
                : base()
            {
            }
            public override void Update(float elapsedTime)
            {
                //apply gravity
                ApplyForce(Gravity);
                base.Update(elapsedTime);
            }
        }
        private class ScoreParticle : AwesomeParticle
        {
            public uint Score { get; set; }
            public override void Draw2D(SpriteBatch spriteBatch)
            {
                // TODO Prerender this mofo
                Vector2 origin = Util.GlobalFont.MeasureString(Score.ToString()) / 2f;
                spriteBatch.DrawText(Position, Score.ToString(), Color, Depth, origin, Scale, Rotation, SpriteEffects.None);
                //spriteBatch.DrawRectangle(new Rectangle((int)Position.X, (int)Position.Y, 2, 2), Microsoft.Xna.Framework.Color.Red);
            }
        }

        public void BarfScore(uint bonus)
        {
            Vector2 pos = new Vector2(Area.Center.X, Area.Center.Y);
            Vector2 v = new Vector2(Util.Random.Next(200) + 250, Util.Random.Next(200) + 260) - pos; //basically, move towards somewhere near the center
            ((GameplayFacet)(Facet)).ParticleSystem.AddParticle(
                    new ScoreParticle()
                    {
                        Position = pos,
                        Score = Score.GetVisibleScore(bonus),
                        MaxAge = 1.8F,
                        InitScale = 0.5F,
                        EndScale = 1.5f,
                        InitColor = colors[bonus % NUM_BLOCK_TYPES].ToVector4(),
                        EndColor = new Vector4(0, 0, 0, 0),
                        Velocity = v,
                        Gravity = new Vector2(0, 0),
                        
                    }
                );
        }
    }
}
