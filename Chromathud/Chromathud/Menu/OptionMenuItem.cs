using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChromathudWin;
using Chromathud;

namespace ChromathudWin.Menu
{
    /// <summary>
    /// A menu item that toggles a certain game option on or off
    /// </summary>
    class OptionMenuItem : MenuItem
    {
        /// <summary>
        /// Text to represent an option that is enabled
        /// </summary>
        private string ontext = Strings.Enabled;
        /// <summary>
        /// Text to represent an option that is disabled
        /// </summary>
        private string offtext = Strings.Disabled;

        private Type prefType;

        /// <summary>
        /// Is the option enabled?
        /// </summary>
        private bool OptionEnabled
        {
            get { return Preferences.GetBoolean(text); }
            set { Preferences.SetBoolean(text, value); }
        }

        private string PrefValue
        {
            get
            {
                if (prefType == typeof(bool))
                    return OptionEnabled ? ontext : offtext;
                else if (prefType == typeof(float))
                    return Preferences.GetInteger(text).ToString();
                return "";
            }
        }

        public OptionMenuItem(OptionsMenu menu, MenuItemSpec spec)
            : base(menu, spec)
        {
            prefType = Preferences.GetType(text);
        }
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            string temp = text;

            string t = Strings.ResourceManager.GetString(text, Strings.Culture);
            if (t == null)
                t = text;
            text = t + ": " + PrefValue;

            base.Draw(gameTime);
            text = temp;
        }
        public override void Activate()
        {
            if (prefType == typeof(bool))
                OptionEnabled = !OptionEnabled;

            else if (prefType == typeof(int))
            {
                int val = Preferences.GetInteger(text);
                Preferences.SetInteger(text, val + 1);
            }
        }
    }
}
