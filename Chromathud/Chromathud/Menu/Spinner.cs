using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChromathudWin.Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ChromathudWin.Menu
{
    public class Spinner : MiniGameComponent, IMenuItem
    {
        private Rectangle location;
        private Rectangle leftarrow;
        private Rectangle rightarrow;
        private bool leftselected;
        private bool rightselected;
        private Texture2D leftselsprite;
        private Texture2D leftdeselsprite;
        private Texture2D rightselsprite;
        private Texture2D rightdeselsprite;

        public string Label { get; set; }
        public int Number { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        
        public const int Padding = 5;

        public Spinner(GameMenu menu)
            : base(menu)
        {
            this.Menu = menu;

            leftselsprite = Content.LoadImage("mnu/LArrowClicked");
            leftdeselsprite = Content.LoadImage("mnu/LArrowUnclicked");
            rightselsprite = Content.LoadImage("mnu/RArrowClicked");
            rightdeselsprite = Content.LoadImage("mnu/RArrowUnclicked");

            rightarrow.Width = rightselsprite.Width;
            rightarrow.Height = rightselsprite.Height;
            leftarrow.Width = leftselsprite.Width;
            leftarrow.Height = leftselsprite.Height;
            Location = Rectangle.Empty;
            Min = 0;
            Number = 0;
            Max = int.MaxValue;
        }

        #region IMenuItem Members

        public bool Selected
        {
            get;
            set;
        }

        public Microsoft.Xna.Framework.Rectangle Location
        {
            get 
            { 
                return location; 
            }
            set
            {
                location = value;
                leftarrow.X = location.X;
                rightarrow.X = location.Right - rightarrow.Width;

                leftarrow.Y = rightarrow.Y = location.Bottom - leftselsprite.Height; ;
            }
        }

        public AbstractMenu Menu { get; private set; }

        public void Activate()
        {
            
        }

        #endregion

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {

            if (Selected)
            {
                if (InputManager.GetBoolean(UserCommand.MenuLeft))
                    goLeft();
                if (InputManager.GetBoolean(UserCommand.MenuRight))
                    goRight();
            }


            Point p = new Point(Mouse.GetState().X, Mouse.GetState().Y);
            leftselected = rightselected = false;
            if (leftarrow.Contains(p))
                leftselected = true;
            else if (rightarrow.Contains(p))
                rightselected = true;

            if (MouseDelta.GetState().WasLeftButtonPressed)
            {
                if (leftselected)
                    goLeft();
                if (rightselected)
                    goRight();

            }
        }

        private void goRight()
        {
            if (++Number > Max)
                Number = Max;
        }

        private void goLeft()
        {
            if (--Number < Min)
                Number = Min;
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            SpriteBatch.Draw(Selected || leftselected ? leftselsprite : leftdeselsprite, leftarrow, Color.White);
            SpriteBatch.Draw(Selected || rightselected ? rightselsprite : rightdeselsprite, rightarrow, Color.White);

            Rectangle innerrect = new Rectangle(
                leftarrow.Right + Padding, 
                leftarrow.Y, 
                location.Width - leftarrow.Width - rightselsprite.Width - Padding, 
                leftarrow.Height
            );
//            innerrect.Height /= 2;
            SpriteBatch.DrawTextScaled(innerrect, Label + ":", -1, 0);
            //innerrect.Y += innerrect.Height;
            SpriteBatch.DrawTextScaled(innerrect, Number.ToString(), 1, 0);
        }
    }
}
