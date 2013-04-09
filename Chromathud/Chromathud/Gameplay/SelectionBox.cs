using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChromathudWin.Gameplay
{
    public class SelectionBox : MiniGameComponent
    {
        /// <summary>
        /// The play field associated with the box
        /// </summary>
        public Field Field { get; private set; }
        /// <summary>
        /// The area occupied
        /// </summary>
        public Rectangle Area { get; private set; }

        private Texture2D slotBackground;

        public SelectionBox(Field field, Rectangle area)
            : base(field.Facet)
        {
            Field = field;
            Area = area;
            Initialize();
        }
        public override void Initialize()
        {
            base.Initialize();
            string p = (Field.Player == InputManager.ActivePlayer ? "" :
                Util.IsAlmostEaster() ? "/EE" :
                "/2P");
            slotBackground = Content.LoadImage("TileImages/" + p + "/SelectionBoxBackground");
        }
        public override void Update(GameTime gameTime)
        {
            
        }
        public override void Draw(GameTime gameTime)
        {
            if (Util.UseXboxUI())
            {
                drawXbox(gameTime);
            }
            else 
            {
                drawPC(gameTime);
            }
        }

        private void drawXbox(GameTime gameTime)
        {
            Rectangle dest = Area;
            dest.Width = Block.Width;
            dest.Height = Block.Height;

            //draw each selected block
            for (int i = 0; i < Field.SelectLimit; ++i)
            {
                double j;
                int k;
                if (i < 6)
                {
                    j = i;
                    k = 0;
                }
                else
                {
                    j = 1.5 + i % 6;
                    k = 1;
                }
                dest.X = (int)(Area.X + Block.Width * j);
                dest.Y = Area.Bottom - Block.Height * (k + 1);

                SpriteBatch.Draw(slotBackground, dest, Color.Gray);
                if (i < Field.SelectedBlocks.Count)
                    Field.SelectedBlocks[i].DrawCopyAt(gameTime, dest);
            }
        }
        private void drawPC(GameTime gameTime)
        {
            Rectangle dest = Area;
            dest.Width = Block.Width;
            dest.Height = Block.Height;

            int i = 0;
            //draw each selected block
            for (; i < Field.SelectLimit; ++i)
            {
                int j, k;
                if (Field.SelectLimit <= 5)
                {
                    j = i;
                    k = 0;
                }
                else
                {
                    j = i % 3;
                    k = i / 3;
                }
                dest.X = Area.X + Block.Width * j;
                dest.Y = Area.Bottom - Block.Height * (k + 1);

                SpriteBatch.Draw(slotBackground, dest, Color.Gray);
                if (i < Field.SelectedBlocks.Count)
                    Field.SelectedBlocks[i].DrawCopyAt(gameTime, dest);
            }
        }
    }
}
