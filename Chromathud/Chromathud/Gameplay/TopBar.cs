using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChromathudWin.Gameplay
{
    public class TopBar : MiniGameComponent
    {
        /// <summary>
        /// The play field associated with this bar
        /// </summary>
        public Field Field { get; private set; }
        /// <summary>
        /// The area the bar occupies
        /// </summary>
        public Rectangle Area { get; private set; }

        private Rectangle fillArea;

        private Texture2D image;
        private List<Block> selection;
        private float fade;
        private float FADE_FACTOR = .95F;
        private int goal;

        //colors for tick marks
        private Color tick2color = Color.DarkGray;
        private Color tick1color = Color.White;
        private Color overlimitcolor = Color.White;

        public TopBar(Field field, Rectangle area)
            : base(field.Facet)
        {
            Field = field;
            LoadContent();
            int diff = image.Width - area.Width;
            this.Area = new Rectangle()
            {
                X = area.X - diff / 2 - 1,
                Y = area.Y - 8,
                Width = image.Width,
                Height = image.Height
            };


            selection = new List<Block>(Field.SelectedBlocks);
        }

        public void LoadContent()
        {
            string p = (!Field.Player.HasValue ? "/2P" :
                Field.Player.Value == InputManager.ActivePlayer ? "" :
                Util.IsAlmostEaster() ? "/EE" :
                "/2P");

            image = Content.LoadImage("TileImages/" + Block.Width + p + "/SelectionBar");
            // Get fill area
            Texture2D mask = Content.LoadImage("TileImages/" + Block.Width + "/SelectionBarMask");
            this.fillArea = Util.FindRectInMask(mask, new Color(0xda, 0xbe, 0xef));
            fillArea.X += Area.X;
            fillArea.Y += Area.Y;
        }
        public override void Update(GameTime gameTime)
        {
            /**
             * Update selection if the target was reached. We do this
             * in order to fade the old selection visual after
             * it has been cleared
             */
            if (Field.SelectionSize > 0)
            {
                if (fade != 255F)
                {
                    selection.Clear();
                }
                if (selection.Count != Field.SelectedBlocks.Count)
                {
                    selection = new List<Block>(Field.SelectedBlocks);
                }
                fade = 255F;
                goal = Field.Target;
            }
            else
            {                
                fade *= FADE_FACTOR;
            }
            
        }

        public override void Draw(GameTime gameTime)
        {

            /**
             * Fill with different colors based on selection
             */
            Rectangle dest = fillArea;

            //solid red if over limit
            if (Field.SelectedBlocks.Sum(block => block.Number) > goal)
            {
                SpriteBatch.DrawRectangle(fillArea, overlimitcolor);
            }
            else
            {
                int total = 0;
                foreach (Block b in selection)
                {
                    int cur = total;
                    total += b.Number;
                    dest.X = fillArea.X + cur * fillArea.Width / goal;
                    dest.Width = b.Number * fillArea.Width / goal + 1;

                    Color color = b.Color;
                    int r = color.R * (byte)fade;
                    int g = color.G * (byte)fade;
                    int bl = color.B * (byte)fade;
                    color.R = (byte)(r / 256);
                    color.G = (byte)(g / 256);
                    color.B = (byte)(bl / 256);
                    color.A = (byte)(int)fade;

                    SpriteBatch.DrawRectangle(dest, color);
                    dest.X += dest.Width;
                }
            }

            /**
             * Draw Tick marks
             */
            for (int i = 1; i < Field.Target; ++i)
            {
                bool five = i % 5 == 0; //every fifth tick is more prominent 
                if (!five && Field.Target > 20)
                    continue;

                Rectangle rect = new Rectangle()
                {
                    X = fillArea.X + i * fillArea.Width / Field.Target,
                    Y = fillArea.Y, 
                    Width = five ? 2 : 1,
                    Height = 30
                };
                Color color = five ? tick1color : tick2color;
                SpriteBatch.DrawRectangle(rect, color);
            }

            /**
             * Draw frame
             */
            SpriteBatch.Draw(image, Area, Color.White);

        }
    }
}
