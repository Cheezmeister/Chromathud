using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using ChromathudWin.Menu;
using Microsoft.Xna.Framework;

namespace ChromathudWin.Gameplay
{
    class PauseScreen : Menu.GameMenu
    {
        public PauseScreen(FacetManager fm)
            : base(fm)
        {
        }

        protected override void AddItems()
        {
            MenuItemSpec resume = new MenuItemSpec("Resume", () => Exit(), Color.White) { hovercolor = Color.Cyan };
            MenuItemSpec exit = new MenuItemSpec("Main Menu", exitToMainMenu, Color.White) { hovercolor = Color.Red };

            AddItem(new MenuItem(this, resume));
            AddItem(new MenuItem(this, exit));
            selectedItem = items[0];
        }
        protected override void HandleInput()
        {
            base.HandleInput();
            if (InputManager.GetBoolean(UserCommand.GameUnpause))
                Exit();
        }
        protected override void LoadContent()
        {
            base.LoadContent();
            title = Content.LoadImage("mnu/Pause");
        }

        private void exitToMainMenu()
        {
            FacetManager.PopFacet(null);
            FacetManager.PopFacet(null); // HACK exiting on gameplay's behalf
        }
    }
}
