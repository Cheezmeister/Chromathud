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
using ChromathudWin.Menu;
using ChromathudWin.Gameplay;

namespace ChromathudWin.Menu
{
    public delegate void MenuAction();

    public struct MenuItemSpec
    {
        public String text;
        public MenuAction command;
        public Color normalcolor;
        public Color hovercolor;
        public MenuItemSpec(String text, MenuAction command, Color color)
        { this.text = text; this.command = command; this.normalcolor = this.hovercolor = color; }
    }

    /// <summary>
    /// An item in a menu, duh.
    /// </summary>
    public class MenuItem : MiniGameComponent, IMenuItem
    {
        private bool hovered;
        private bool armed;
        private bool transitioning;
        private float scale;
        private MenuAction command;
        private Color textcolor;
        private Color hovercolor;
        private FacetManager fm;
        private GameMenu gm;
        private Texture2D selSprite;
        private Texture2D deselSprite;
        private Rectangle location;
        protected String text;
        private bool wasHovered;

        #region Properties
        public new bool Enabled { get; set; }
        public Texture2D SelSprite
        {
            get { return selSprite; }
            set { selSprite = value; }
        }
        public Texture2D Icon { get; set; }
        public Texture2D DeselSprite
        {
            get { return deselSprite; }
            set { deselSprite = value; }
        }

        
        public Rectangle Location
        {
            get { return location; }
            set { location = value; }
        }
        public AbstractMenu Menu
        {
            get { return gm; }
        }
        public bool Selected
        {
            get { return hovered; }
            set 
            {
                if (hovered != value)
                {
                    hovered = value;
                    transitioning = true;
                    if (hovered)
                        SoundManager.PlaySound(SFX.Tick, true);
                }
            }
        }
        SpriteFont Font
        {
            get { return Menu.Font; }
        }
        bool TransitioningOn
        {
            get { return hovered && transitioning; }
        }
        bool TransitioningOff
        {
            get { return !hovered && transitioning; }
        }
        #endregion

        public MenuItem(GameMenu gm, MenuItemSpec spec)
            : base(gm)
        {
            this.gm = gm;
            this.fm = gm.FacetManager;
            text = spec.text;
            command = spec.command;
            textcolor = spec.normalcolor;
            hovercolor = spec.hovercolor;
            scale = 1.0f;
            location = Rectangle.Empty;
            selSprite = Menu.Game.Content.LoadImage("mnu/TitleButtonSelected");
            deselSprite = Menu.Game.Content.LoadImage("mnu/TitleButtonDeselected");
            location.Width = selSprite.Width;
            location.Height = selSprite.Height;
            Enabled = true;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (TransitioningOn)
            {
                scale += 0.1F;
                if (scale >= 1.5F)
                    transitioning = false;
            }
            else if (TransitioningOff)
            {
                scale -= 0.1F;
                if (scale <= 1.0F)
                {
                    scale = 1.0F;
                    transitioning = false;
                }
            }

            if (testMouseHover())
            {
                if (InputManager.GetBoolean(UserCommand.MenuCursorMoved))
                    gm.Select(this);
                if (InputManager.GetBoolean(UserCommand.MenuArm))
                    armed = true;
                if (armed && InputManager.GetBoolean(UserCommand.MenuFire))
                {
                    Activate();
                }
            }
        }

        private bool testMouseHover()
        {
#if XBOX
            return false; //no mouse on Xbox!
#else
            return (InputManager.GetFloat(UserCommand.MenuCursorY, PlayerIndex.One) > Location.Y &&
                    InputManager.GetFloat(UserCommand.MenuCursorY, PlayerIndex.One) < Location.Y + Location.Height &&
                    InputManager.GetFloat(UserCommand.MenuCursorX, PlayerIndex.One) > Location.X &&
                    InputManager.GetFloat(UserCommand.MenuCursorX, PlayerIndex.One) < Location.X + Location.Width);
#endif   
        }

        public override void Draw(GameTime gameTime)
        {
            Texture2D sprite = Selected ? selSprite : deselSprite;
            fm.SpriteBatch.Draw(sprite, location, Enabled ? Color.White : Color.Gray);

            //center the string on the button
            Vector2 dim = Font.MeasureString(text);
            Rectangle textloc = location;
            textloc.Inflate(-18, -24);

            //draw icon if any
            if (Icon != null)
            {
                Rectangle dest = textloc;
                dest.Width = Icon.Width;
                SpriteBatch.Draw(Icon, dest, Color.White);
                textloc.X = dest.Right; //text to right of icon
                textloc.Width -= Icon.Width;
            }

            //choose a text color
            Color color = Selected ? hovercolor : textcolor;
            SpriteBatch.DrawTextScaled(textloc, text, 0, 0, color, Font);

        }

        public virtual void Activate()
        {
            if (!Enabled)
            {
                SoundManager.PlaySound(SFX.CannotSelect, true);
                return;
            }

            SoundManager.PlaySound(SFX.Select, true);
            command();
        }

    }
}