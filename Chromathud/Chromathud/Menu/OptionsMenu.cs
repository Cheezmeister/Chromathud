using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ChromathudWin.Menu
{
    class OptionsMenu : GameMenu
    {
        // How many rows/columns of buttons will fit on the screen
        private const int MAX_ROWS = 5;
        private const int MAX_COLUMNS = 2;

        public OptionsMenu(FacetManager fm)
            : base(fm)
        {
        }
        protected override void LoadContent()
        {
            base.LoadContent();
            title = Content.LoadImage("mnu/Options");
        }
        protected override void AddItems()
        {
            Color[] dasList = {
                                Color.Orange,
                                Color.Purple,
                                Color.GreenYellow,
                                Color.Crimson,
                                Color.Salmon
                            };
            int i = 0;
            foreach (string s in Preferences.List(null))
            {

                MenuItemSpec spec = new MenuItemSpec(s, () => { }, dasList[i++ % dasList.Length]) { normalcolor = Color.White };
                OptionMenuItem item = new OptionMenuItem(this, spec);
                AddItem(item);
            }

            MenuItemSpec back = new MenuItemSpec("Back", () => Exit(), Color.Red) { normalcolor = Color.White };
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
    }
}
