using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Xna.Framework;
using EasyStorage;
using System.Diagnostics;

namespace ChromathudWin
{

    /// <summary>
    /// Preferences for Chromathud
    /// </summary>
    public class Preferences : AbstractPreferences<string>
    {
        /// <summary>
        /// A token to identify load operations that belong to us
        /// </summary>
        private const int LOAD_COOKIE = 42;
#if DEBUG
        private const string PATH = "debug_prefs.xml";
#else
        private const string PATH = "prefs.xml";
#endif
        private static Preferences Instance
        {
            get { return ((Preferences)instance); }
        }

        protected Preferences()
            : base()
        {
        }

        protected override void loadDefaults()
        {
            //hard-coded defaults
#if DEBUG
            setDefault("DebugMode", false);
#endif
            setDefault("Sound", true);
            setDefault("Music", true);
            setDefault("TargetNotifications", true);
        }

        /// <summary>
        /// Begin to save preferences to file using the EasyStorage API
        /// </summary>
        /// <returns></returns>
        public static bool SaveAsync(IAsyncSaveDevice saveDevice)
        {
            saveDevice.Save(ChromathudGame.Name, PATH, stream => Instance.saveToFile(stream));
            return true;
        }
        /// <summary>
        /// Begin to load preferences from file with EasyStorage
        /// </summary>
        /// <param name="saveDevice"></param>
        /// <returns></returns>
        public static bool LoadAsync(IAsyncSaveDevice saveDevice, LoadCompletedEventHandler loadCompletedEventHandler)
        {
            int cookie = LOAD_COOKIE;
            instance = new Preferences();

            saveDevice.LoadCompleted += new LoadCompletedEventHandler(handleLoadCompleted);
            if (loadCompletedEventHandler != null)
                saveDevice.LoadCompleted += loadCompletedEventHandler;
            saveDevice.LoadAsync(ChromathudGame.Name, PATH, stream => Instance.loadFromFile(stream), cookie);
            return true;
        }
        /// <summary>
        /// Begin to load preferences from file with EasyStorage
        /// </summary>
        /// <param name="saveDevice"></param>
        /// <returns></returns>
        public static bool LoadAsync(IAsyncSaveDevice saveDevice)
        {
            return LoadAsync(saveDevice, null);
        }

        private static void handleLoadCompleted(object sender, FileActionCompletedEventArgs ea)
        {
            if ((int)ea.UserState != LOAD_COOKIE)
                return;
            if (ea.Error != null)
            {
                Console.WriteLine("No prefs file found, using defaults");
            }
            else
            {
                Console.WriteLine("Preferences loaded OK");
            }
        }

    }
}
