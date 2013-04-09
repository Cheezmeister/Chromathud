using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Chromathud;


namespace ChromathudWin.Menu
{
    /// <summary>
    /// Small screen before play to choose difficulty from 1-9
    /// This is hacked somewhat to provide two levels of menu, instead of nesting two separate menus.
    /// </summary>
    public class GameSettingsMenu : GameMenu
    {
        public int startLevel;
        Gameplay.GameplayFacet game;

        public GameSettingsMenu(Gameplay.GameplayFacet gf)
            : base(gf.FacetManager)
        {
            game = gf;
        }
        protected override void LoadContent()
        {
            base.LoadContent();
            title = Content.LoadImage("mnu/Select");
        }
        protected override void AddItems()
        {
            MenuItemSpec mode = new MenuItemSpec(Strings.TimedMode, () => { ChooseTime(); }, Color.Orange) { normalcolor = Color.White };
            MenuItemSpec mode2 = new MenuItemSpec(Strings.EndlessMode, () => { ChooseLevel(); }, Color.Purple) { normalcolor = Color.White }; 
            MenuItemSpec tut = new MenuItemSpec(Strings.Tutorial, () => { StartTutorial(); }, Color.Green) { normalcolor = Color.White }; 

            MenuItem one = new MenuItem(this, mode);
            MenuItem two = new MenuItem(this, mode2);
            MenuItem three = new MenuItem(this, tut);

            title = Content.LoadImage("mnu/Select");

            AddItem(two);
            AddItem(one);
            AddItem(three);
        }

        private void StartTutorial()
        {
            game.IsTutorialMode = true;
            startGame();
        }

        private void ChooseLevel()
        {
            ClearItems();
            game.GameplayMode = ChromathudWin.Gameplay.GameplayMode.Endless;
            Spinner spin = new Spinner(this) { Label = "Level", Min = 1, Number = 1, Max = 7 };
            AddItem(spin);
            MenuItemSpec go = new MenuItemSpec(Strings.Play, () => { game.Level = spin.Number; startGame(); }, Color.White);
            AddItem(new MenuItem(this, go));
            Select(items.Last());
            base.SortItems();
        }

        private void ChooseTime()
        {
            ClearItems();
            game.GameplayMode = ChromathudWin.Gameplay.GameplayMode.Timed;
            Spinner spin = new Spinner(this) { Label = "Minutes", Min = 1, Number = 1, Max = 10 };
            AddItem(spin);
            MenuItemSpec go = new MenuItemSpec(Strings.Play, () => { game.MinuteLimit = spin.Number; startGame(); }, Color.White);
            AddItem(new MenuItem(this, go));
            Select(items.Last());
            base.SortItems();
        }

        private void startGame()
        {
            FacetManager.ReplaceMeWith(game, new FadeTransition(200));
        }
        public override void Initialize()
        {
            base.Initialize();
            startLevel = 1;
        }

        protected override void SortItems()
        {
            tableLayout(3, 1);
        }
        public string LevelString()
        {
            string ret = startLevel.ToString();
            if (startLevel == 0)
                ret += " (Tutorial)";
            return ret;
        }
    }
}