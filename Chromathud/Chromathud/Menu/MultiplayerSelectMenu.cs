using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Chromathud;

namespace ChromathudWin.Menu
{
    class MultiplayerSelectMenu : GameMenu
    {
        // How many rows/columns of buttons will fit on the screen
        private const int MAX_ROWS = 5;
        private const int MAX_COLUMNS = 2;
        Gameplay.GameplayFacet game;

        public MultiplayerSelectMenu(Gameplay.GameplayFacet gf)
            : base(gf.FacetManager)
        {
            game = gf;
        }
        protected override void AddItems()
        {
            // Load one of the four different types of multiplayer
            MenuItemSpec mp1 = new MenuItemSpec("Type 1", () => { ChooseType1(); }, Color.White);
            MenuItemSpec mp2 = new MenuItemSpec("Type 2", () => { ChooseType2(); }, Color.White);
            MenuItemSpec mp3 = new MenuItemSpec("Type 3", () => { ChooseType3(); }, Color.White);
            MenuItemSpec mp4 = new MenuItemSpec("Type 4", () => { ChooseType4(); }, Color.White);

            MultiplayerSelectMenuItem typeOne = new MultiplayerSelectMenuItem(this, mp1);
            MultiplayerSelectMenuItem typeTwo = new MultiplayerSelectMenuItem(this, mp1);
            MultiplayerSelectMenuItem typeThree = new MultiplayerSelectMenuItem(this, mp1);
            MultiplayerSelectMenuItem typeFour = new MultiplayerSelectMenuItem(this, mp1);

            title = Content.LoadImage("Select");

            Rectangle tsa = game.FacetManager.Game.GraphicsDevice.Viewport.TitleSafeArea;
            int x = tsa.Center.X - typeOne.SelSprite.Width / 2;
            typeOne.Location = new Rectangle(x, tsa.Center.Y - typeOne.SelSprite.Height, typeOne.SelSprite.Width, typeOne.SelSprite.Height);
            x = tsa.Center.X - typeTwo.SelSprite.Width / 2;
            typeTwo.Location = new Rectangle(x, tsa.Center.Y - typeTwo.SelSprite.Height, typeTwo.SelSprite.Width, typeTwo.SelSprite.Height);
            x = tsa.Center.X - typeThree.SelSprite.Width / 2;
            typeThree.Location = new Rectangle(x, tsa.Center.Y - typeThree.SelSprite.Height, typeThree.SelSprite.Width, typeThree.SelSprite.Height);
            x = tsa.Center.X - typeFour.SelSprite.Width / 2;
            typeFour.Location = new Rectangle(x, tsa.Center.Y - typeFour.SelSprite.Height, typeFour.SelSprite.Width, typeFour.SelSprite.Height);

            MenuItemSpec back = new MenuItemSpec("Back", () => Exit(), Color.Magenta);
            AddItem(new MenuItem(this, mp1));
            AddItem(new MenuItem(this, mp2));
            AddItem(new MenuItem(this, mp3));
            AddItem(new MenuItem(this, mp4));
            AddItem(new MenuItem(this, back));
        }

        protected override void SortItems()
        {
            int vertical, horizontal;
            if (items.Count <= MAX_ROWS)
            {
                vertical = MAX_ROWS;
                horizontal = 1;
            }
            else
            {
                vertical = (items.Count + 1) / 2;
                horizontal = 2;
            }
            tableLayout(vertical, horizontal);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            title = Content.LoadImage("MultiSelect");
        }

        private void ChooseType1()
        {
            ClearItems();
            game.GameplayMode = ChromathudWin.Gameplay.GameplayMode.Timed;
            Spinner spin = new Spinner(this) { Label = "Minutes", Min = 1, Number = 1, Max = 10 };
            AddItem(spin);
            MenuItemSpec go = new MenuItemSpec(Strings.Play, () => { game.MinuteLimit = spin.Number; startGame(); }, Color.White);
            AddItem(new MenuItem(this, go));
            selectedItem = items.Last();
            selectedItem.Selected = true;
            base.SortItems();
        }
        private void ChooseType2()
        {
        }
        private void ChooseType3()
        {
        }
        private void ChooseType4()
        {
        }

        private void startGame()
        {
            Exit();
            FacetManager.AddFacet(game);
        }
    }
}
