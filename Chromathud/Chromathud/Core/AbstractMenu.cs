using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ChromathudWin
{
    public interface IMenuItem : IGameComponent
    {
        bool Enabled { get; set; }
        bool Selected { get; set; }
        Rectangle Location { get; set; }
        AbstractMenu Menu { get; }
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
        void Activate();
    }

    /// <summary>
    /// Base menu class. 
    /// </summary>
    public abstract class AbstractMenu : DrawableGameComponent
    {
        protected List<IMenuItem> items; //the menu items currently available
        protected IMenuItem selectedItem; //the item that's currently selected
        private SpriteBatch spriteBatch; //the spriteBatch to use for drawing
        private SpriteFont font; //the font to use 
        protected int selectionIndex; //the index of the selected item 
        private int oldIndex; //the index formerly selected
        private string titletext;

        #region Properties
        /// <summary>
        /// The font used by the menu
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
            set { font = value; }
        }
        public ContentManager Content
        {
            get { return Game.Content; }
        }
        /// <summary>
        /// The title to display
        /// </summary>
        public string Title
        {
            get { return titletext; }
            set { titletext = value; }
        }
        /// <summary>
        /// A SpriteBatch to use for drawing
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
            set { spriteBatch = value; }
        }

        public int LineHeight { get; set; }

        #endregion

        public AbstractMenu(Game fm, SpriteBatch sb)
            : base(fm)
        {
            spriteBatch = sb;
            items = new List<IMenuItem>();
            selectionIndex = 0;
        }

        public override void Initialize()
        {
            //show the cursor for menus
            Game.IsMouseVisible = true;

            AddItems();
            SortItems();
           // ValidateSorting();

            if (items.Count > 0)
                Select(items[0]);

            base.Initialize();
        }
        public void ClearItems()
        {
            items.Clear();
        }

        /// <summary>
        /// Populate the menu's entries
        /// </summary>
        protected abstract void AddItems();
        
        /// <summary>
        /// Determine the layout of menu items
        /// </summary>
        protected virtual void SortItems()
        {
            Vector2 buttonStart = Vector2.Zero;

            int columns = 1;

            //start buttons in the center of the screen
            Rectangle tsa = Game.GraphicsDevice.Viewport.TitleSafeArea;
            buttonStart.X = tsa.Center.X;
            buttonStart.Y = tsa.Top + 32;
            foreach (IMenuItem item in items)
            {
                item.Location = new Rectangle((int)buttonStart.X, (int)buttonStart.Y, 0, 0);
                buttonStart.Y += LineHeight;
            }
        }

        /// <summary>
        /// Ensure all items are visible on-screen
        /// </summary>
        protected virtual void ValidateSorting()
        {
            Rectangle tsa = Game.GraphicsDevice.Viewport.TitleSafeArea;
    
            //get a bounding rectangle for all items
            Rectangle overall = new Rectangle();

            overall.X = items[0].Location.X;
            overall.Y = items[0].Location.Y;
            overall.Width = items[0].Location.Right - overall.X;
            overall.Height = items[items.Count - 1].Location.Bottom - overall.Y; ;

            //if it's not entirely within the title safe area, scale it
            if (!tsa.Contains(overall))
            {
                float xscale = Math.Min(1, overall.Width / (float)tsa.Width);
                float yscale = Math.Min(1, overall.Height / (float)tsa.Height);

                foreach (IMenuItem item in items)
                {
                    Rectangle loc = item.Location;
                    if (xscale < 1.0F)
                    {
                        float tempx = tsa.Center.X - item.Location.X;
                        tempx *= xscale;
                        
                        loc.X = tsa.Center.X - (int)tempx;
                        loc.Width = (int)((float)loc.Width * xscale);
                    }
                    if (yscale < 1.0F)
                    {
                        float tempy = tsa.Center.Y - item.Location.Y;
                        tempy *= yscale;
                        loc.Y = tsa.Center.Y - (int)tempy;
                        loc.Height = (int)((float)loc.Height * yscale);
                    }
                    item.Location = loc;
                }
                
            }
        }
      
        public override void Update(GameTime gameTime)
        {
            HandleInput();
            Util.Foreach<IMenuItem>(items, item =>
            {
                item.Update(gameTime);
            });

            base.Update(gameTime);
        }

        protected virtual void HandleInput()
        {
            if (KeyboardDelta.WasKeyPressed(Keys.Up))
                ChangeIndex(false);
            else if (KeyboardDelta.WasKeyPressed(Keys.Down))
                ChangeIndex(true);
            if (KeyboardDelta.WasKeyPressed(Keys.Enter))
            {
                selectedItem.Activate();
            }
        }

        /// <summary>
        /// Select either the next or previous item in the menu
        /// </summary>
        /// <param name="forward">
        /// If true, the next item is selected. Else the previous is selected.
        /// </param>
        protected void ChangeIndex(bool forward)
        {
            oldIndex = selectionIndex;
            do
            {
                //cycle index
                if (selectionIndex <= 0)
                    selectionIndex = items.Count;
                selectionIndex += (forward ? 1 : -1);
                selectionIndex %= items.Count;
            } while (items[selectionIndex].Enabled == false);
            
            Select(items[selectionIndex]);
            
        }

        public override void Draw(GameTime gameTime)
        {
            Rectangle tsa = Game.GraphicsDevice.Viewport.TitleSafeArea;

            //draw the title in the top left corner
            Vector2 topleft = new Vector2(tsa.X, tsa.Y);
            SpriteBatch.DrawString(Font, titletext, topleft, Color.Black);

            foreach (IMenuItem item in items)
            {
                item.Draw(gameTime);
            }
        }

        public void AddItem(IMenuItem item)
        {
            items.Add(item);
        }
        public void Select(IMenuItem itemToSelect)
        {
            if (!itemToSelect.Enabled)
                return;

            selectedItem = itemToSelect;
            selectedItem.Selected = true;

            foreach (IMenuItem item in items)
            {
                if (item != itemToSelect)
                {
                    item.Selected = false;
                }
                else
                {
                    selectionIndex = items.IndexOf(item);
                }
            }
        }
    }
}
