using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Chromathud;
using ChromathudWin.Gameplay;

namespace ChromathudWin.Menu
{
    /// <summary>
    /// The menu for the game. We'll be taken here when the program
    /// starts and between games. 
    /// </summary>
    class MainMenu : GameMenu
    {
        private IMenuItem multiplay;

        public MainMenu(FacetManager fm) : base(fm) { }

        protected override void AddItems()
        {
            //TODO clean this up, read from file or something
            MenuItemSpec play = new MenuItemSpec(
                Strings.Play, 
                () => StartSinglePlayer(), 
                Color.Violet) { normalcolor = Color.White };
            AddItem(new MenuItem(this, play));

            if (Util.UseXboxUI())
            {
                MenuItemSpec network = new MenuItemSpec(
                    Strings.MultiPlay,
                    () => FacetManager.AddFacet(new MultiplayScreen(FacetManager), new FadeTransition(200)),
                    Color.Orange) { normalcolor = Color.White };
                MenuItemSpec splitscreen = new MenuItemSpec(
                    "Splitscreen",
                    () => FacetManager.AddFacet(new GameplayFacet(FacetManager) { SinglePlayer = false }, new FadeTransition(200)),
                    Color.DarkOrange) { normalcolor = Color.White };
#if XBOX
                //AddItem(multiplay = new MenuItem(this, network) { Enabled = !Guide.IsTrialMode } );
#endif
                AddItem(new MenuItem(this, splitscreen));
            }

            MenuItemSpec options = new MenuItemSpec(
                Strings.Options, 
                () => FacetManager.AddFacet(new OptionsMenu(FacetManager), new FadeTransition(200)), 
                Color.Yellow) { normalcolor = Color.White };
            MenuItemSpec how = new MenuItemSpec(
                Strings.HowToPlay, 
                () => FacetManager.AddFacet(new Gameplay.Instructions(FacetManager), new FadeTransition(200)), 
                Color.Cyan) { normalcolor = Color.White };
            MenuItemSpec quit = new MenuItemSpec(
                Strings.Quit, 
                () => InputManager.PostEvent(UserCommand.ExitProgram, PlayerIndex.One), 
                Color.Red) { normalcolor = Color.White };
            MenuItemSpec credits = new MenuItemSpec(
                Strings.Credits, 
                () => FacetManager.AddFacet(new Credits(FacetManager), new FadeTransition(200)), 
                Color.Blue) { normalcolor = Color.White };

            AddItem(new MenuItem(this, options));
            AddItem(new MenuItem(this, how));
            AddItem(new MenuItem(this, credits));
            AddItem(new MenuItem(this, quit));
            selectedItem = items[0];


        }
        protected override void SortItems()
        {
            if (Util.UseWindowsUI())
            {
                tableLayout(5, 1);
            }
            else
            {
                tableLayout(3, 2);
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
#if XBOX
            //multiplay.Enabled = !Guide.IsTrialMode && !Guide.SimulateTrialMode;
#endif
        }

        protected override void HandleInput()
        {
            base.HandleInput();
            if (InputManager.GetBoolean(UserCommand.MenuLeft) || InputManager.GetBoolean(UserCommand.MenuRight))
            {
                int newindex = (selectionIndex + 3) % items.Count;
                this.Select(items[newindex]);
            }
        }
        protected override void LoadContent()
        {
            base.LoadContent();
            title = Content.LoadImage("mnu/Title");
        }

        private void StartSinglePlayer()
        {
            FacetManager.AddFacet(new GameSettingsMenu(new GameplayFacet(FacetManager)), new FadeTransition(200));
        }
        private void StartMultiPlayer()
        {
            Gameplay.GameplayFacet gf = new GameplayFacet(FacetManager);
            gf.SinglePlayer = false;
            FacetManager.AddFacet(gf, new FadeTransition(200));
        }

    }
}
