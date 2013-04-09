using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace ChromathudWin
{ 
    /// <summary>
    /// A set of preferences used as game options. Handles saving/loading XML.
    /// This class is meant to be derived with game-specific preferences. 
    /// Implementing classes should implement loadDefaults()
    /// </summary>
    /// <typeparam name="TKey">
    /// The type used to index individual preference values. Usually a string or enum
    /// </typeparam>
    public abstract class AbstractPreferences<TKey>
    {
        public SerializableDictionary<TKey, Object> defaults;
        public SerializableDictionary<TKey, Object> settings;

        //A struct to wrap members for serialization
        public struct PData
        {
            public SerializableDictionary<TKey, Object> d;
            public SerializableDictionary<TKey, Object> s;
        }

        protected static AbstractPreferences<TKey> instance;
        public static bool Valid { get { return instance != null; } }

        /// <summary>
        /// Initialize fields
        /// </summary>
        protected AbstractPreferences()
        {
            defaults = new SerializableDictionary<TKey, Object>();
            settings = new SerializableDictionary<TKey, Object>();
            loadDefaults();
        }
        /// <summary>
        /// List names of all available options
        /// </summary>
        /// <returns></returns>
        public static TKey[] List(Type t)
        {
            if (t == null)
            {
                SerializableDictionary<TKey, Object>.KeyCollection keys = instance.defaults.Keys;
                ICollection<TKey> morekeys = instance.settings.Keys.ToArray<TKey>();
                return keys.Union(morekeys).ToArray();
            }
            return new TKey[0];
        }
        /// <summary>
        /// Set a hard-coded default for a preference. 
        /// This should generally only be called once on startup
        /// </summary>
        /// <param name="name">The preference to set</param>
        /// <param name="value">The value to assign</param>
        public static void SetDefault(TKey name, Object value)
        {
            instance.setDefault(name, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Type GetType(TKey text)
        {
            Object val = instance.internalGet(text);
            return val == null ? null : val.GetType();
        }
        /// <summary>
        /// Reset all values to defaults
        /// </summary>
        public static void LoadDefaults()
        {
            instance.loadDefaults();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetBoolean(TKey name, bool value)
        {
            return instance.internalSet(name, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetBoolean(TKey name)
        {
            if (!Valid)
                return false;

            try
            {
                Object ret = instance.internalGet(name);
                if (ret == null)
                    ret = false;

                return (bool)ret;
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("Preference " + name.ToString() + " is not of boolean type");
                return false;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine(instance == null?"Instance is null":"NullReferenceException");
                return false;
            }
        }
        /// <summary>
        /// Set an integer preference
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetInteger(TKey name, int value)
        {
            return instance.internalSet(name, value);
        }
        /// <summary>
        /// Get an integer preference
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetInteger(TKey name)
        {
            try
            {
                return (int)instance.internalGet(name);
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("Warning: Preference " + name.ToString() + " is not an integer");
                return 0;
            }
        }
        private Object internalGet(TKey name)
        {
            Object ret = null;
            //Hmm, let's see, is this option set?
            if (settings.TryGetValue(name, out ret))
                if (ret != null)
                    return ret;
            
            //if not, look for a default
            if (defaults.TryGetValue(name, out ret))
                return ret;

#if DEBUG
            //hopefully we won't fall through to here >_>;
            Console.WriteLine("Adding unexpected preference " + name);
            defaults.Add(name, ret);
            settings.Add(name, ret);
#else
            //Console.WriteLine("Invalid preference: " + name.ToString() );
            ret = null;
#endif


            return ret;
        }
        /// <summary>
        /// Set the default for a given pref. 
        /// If the preference doesn't exist, it will be added
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void setDefault(TKey name, Object value)
        {
            try
            {
                defaults.Add(name, value);
            }
            catch (ArgumentException)
            {
                //I should really do something here.
                //But I'm not going to...
            }

        }

        /// <summary>
        /// Init (or reset) default preference values. This should 
        /// be implemented by a subclass, using setDefault() to create 
        /// game-specific preferences. <em>All</em> preferences should be
        /// given some default value here.
        /// </summary>
        /// <example>
        /// setDefault("Difficulty", Difficulty.Easy)
        /// setDefault("MusicEnabled", true)
        /// </example>
        protected abstract void loadDefaults();
        /// <summary>
        /// Save to file using an XmlSerializer
        /// </summary>
        /// <returns></returns>
        protected virtual bool saveToFile(Stream stream)
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(PData));
                PData pdata;
                pdata.d = defaults;
                pdata.s = settings;
                ser.Serialize(stream, pdata);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }
        /// <summary>
        /// Load preferences from a prepared stream
        /// </summary>
        /// <param name="stream">A valid stream, the file to load</param>
        /// <returns>true on success, false otherwise</returns>
        protected virtual bool loadFromFile(Stream stream)
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(PData));
                PData d = (PData)(ser.Deserialize(stream));
                defaults = d.d;
                settings = d.s;
            }
           catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }


        private bool internalSet(TKey name, Object value)
        {
            Object oldval;

            if (settings.TryGetValue(name, out oldval))
                if (oldval == value)
                    return false;
            settings[name] = value;
            return true;
        }
    }

}
