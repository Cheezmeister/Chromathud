using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChromathudWin;
using ChromathudWin.Gameplay;
using Chromathud;
using Microsoft.Xna.Framework.GamerServices;

namespace ChromathudWin.Menu
{
    public abstract class GameMenu : AbstractMenu, IGameFacet
    {
        /// <summary>
        /// The title screen background
        /// </summary>
        protected Texture2D title;
        /// <summary>
        /// The tiled background texture
        /// </summary>
        protected Texture2D background;
        /// <summary>
        /// Are we in a transition from one item to another?
        /// </summary>
        private bool changingSelection;

        public TimeSpan CumulativeTime { get; private set; }

        private FacetManager facetManager;
       
        
        public FacetManager FacetManager
        {
            get { return facetManager; }
            set { facetManager = value; }
        }
        public new SpriteBatch SpriteBatch
        {
            get { return facetManager.SpriteBatch; }
        }
        /// <summary>
        /// Simply call base constructor
        /// </summary>
        /// <param name="fm"></param>
        public GameMenu(FacetManager fm)
            : base(fm.Game, fm.SpriteBatch)
        {
            facetManager = fm;
#if XBOX
            LineHeight = Layout.ButtonHeight - 16; //to account for padding in button images
#else
            LineHeight = Layout.ButtonHeight;
#endif
        }
        public void Cleanup() { }

        /// <summary>
        /// Determine the layout of menu items
        /// </summary>
        protected override void SortItems()
        {
            int rows = Layout.ButtonsPerColumn;
            
            if (items.Count == rows + 1)
                ++rows; //special exception to avoid "hanging" items

            int columns = (items.Count - 1) / rows + 1;
            tableLayout(rows, columns);

        }
        protected void tableLayout(int rows, int columns)
        {
            Vector2 buttonStart = Vector2.Zero;
        
            //start buttons in the center of the screen
            Rectangle tsa = FacetManager.GraphicsDevice.Viewport.TitleSafeArea;
            buttonStart.Y = tsa.Center.Y - LineHeight * rows / 4;
       
            //for each additional column, the leftmost column is shifted half a width left
            buttonStart.X = tsa.Center.X - (columns * Layout.ButtonWidth / 2);
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < columns; ++j)
                {
                    int index = j * rows + i;
                    if (index >= items.Count)
                        break;

                    IMenuItem item = items[index];
                    int y = (int)buttonStart.Y + LineHeight * (i);
                    int x = (int)buttonStart.X + Layout.ButtonWidth * j;
                    item.Location = new Rectangle(x, y, Layout.ButtonWidth, Layout.ButtonHeight);
                } 
            }
        }

        protected override void LoadContent()
        {
            Font = Content.LoadFont("chroma");
            background = Content.LoadImage("TileImages/BGTile");
        }

        protected override void UnloadContent()
        {
            
        }
        public override void Draw(GameTime gameTime)
        {
            FacetManager.Game.GraphicsDevice.Clear(Color.BurlyWood);
            Rectangle tsa = FacetManager.GraphicsDevice.Viewport.TitleSafeArea;

            //draw the tiled background
            Vector2 location = new Vector2();
            for (int x = 0; x < FacetManager.GraphicsDevice.Viewport.Width; x += background.Width)
            {
                location.X = x;
                for (int y = 0; y < FacetManager.GraphicsDevice.Viewport.Height; y += background.Height)
                {
                    location.Y = y;
                    SpriteBatch.Draw(background, location, Color.White);
                }
            }

            //draw the logo top & centered
            Rectangle dest = title.FitIn(tsa, 0, -1);
            SpriteBatch.Draw(title, dest, Color.White);

            foreach (IMenuItem item in items)
            {
                item.Draw(gameTime);
            }
        }

        // Overriding in order to use Chromathud's freaky-ass input management
        protected override void HandleInput()
        {
            if (InputManager.GetBoolean(UserCommand.MenuDown))
                ChangeIndex(true);
            else if (InputManager.GetBoolean(UserCommand.MenuUp))
                ChangeIndex(false);

            if (InputManager.GetBoolean(UserCommand.MenuSelect))
            {
                selectedItem.Activate();
            }
            if (InputManager.GetBoolean(UserCommand.MenuBack))
            {
                Exit();
            }
        }

        public void Exit()
        {
            FacetManager.PopFacet();
        }
        private void StartSinglePlayer()
        {
            FacetManager.AddFacet(new GameSettingsMenu(new GameplayFacet(FacetManager)));
        }
        private void StartMultiPlayer()
        {
            Gameplay.GameplayFacet gf = new Gameplay.GameplayFacet(FacetManager);
            gf.SinglePlayer = false;
            FacetManager.AddFacet(new MultiplayScreen(FacetManager));
        }
    }
}
