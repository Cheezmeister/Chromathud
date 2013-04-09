using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.Threading;

namespace ChromathudWin
{
    enum SFX
    {
        Select,
        Deselect,
        Clink,
        Tick,
        Sweet,
        CannotSelect,
        Clear,
        Thump,
        NumSounds,
    }
    enum BGM
    {
        Chromathud,
        KillingTime,
        ShortStory,
        Count,
    }

    /// <summary>
    /// Singleton class for sound effect/BGM management
    /// </summary>
    class SoundManager : GameComponent
    {
        private const string PREFIX = "sfx/";
#if WINDOWS
        private const string PLATFORM = "Win/";
#else
        private const string PLATFORM = "Xbox/";
#endif
        private const float MediaVolume = .2F;

        private static SoundManager instance;
        private Boolean NeedToStartBGM = false;
        private Boolean NeedToStopBGM = false;
        private Boolean NeedToExitBGM = false;
        private BGM CurrentBGM;
        private Dictionary<SFX, SoundEffect> sounds;
        private Dictionary<SFX, SoundEffectInstance> playing;
        private Dictionary<SFX, string> names;
        private Dictionary<BGM, string> bgms;

        private Thread bgmThread;


        #region Properties
        protected static SoundManager Instance
        {
            get { return instance; }
        }
        private ContentManager Content
        {
            get { return Game.Content; }
        }
        #endregion

        public static SoundManager GetInstance(Game game)
        {
            if (instance == null)
                instance = new SoundManager(game);
            return instance;

        }
        
        private SoundManager(Game game) : base(game)
        {
            sounds = new Dictionary<SFX, SoundEffect>();
            names = new Dictionary<SFX, string>();
            bgms = new Dictionary<BGM, string>();
            playing = new Dictionary<SFX, SoundEffectInstance>();

            names[SFX.Clink] = "Clink";
            names[SFX.Deselect] = "Deselect";
            names[SFX.Select] = "Select";
            names[SFX.Sweet] = "Sweet";
            names[SFX.Tick] = "chick";
            names[SFX.CannotSelect] = "wrong";
            names[SFX.Clear] = "Clear";
            names[SFX.Thump] = "Thump";

            bgms[BGM.KillingTime] = "Killing Time";
            bgms[BGM.Chromathud] = "Chromathud";
            bgms[BGM.ShortStory] = "Short Story";
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public static void Cleanup()
        {
            Instance.NeedToExitBGM = true;
        }
        public static void PlayBGM(int mode)
        {
            instance.playBGM(mode);            
        }
        public static void PlaySound(SFX which, bool unique)
        {
            if (Preferences.GetBoolean("Sound"))
                Instance.playSound(which, unique);
        }

        public static SoundEffect GetSound(SFX which)
        {
            return Instance.getSound(which);
        }
        private void playBGM(int mode)
        {
            NeedToStopBGM = false;
            if (Preferences.GetBoolean("Sound") && Preferences.GetBoolean("Music"))
            {
                CurrentBGM = (BGM)(mode % (int)BGM.Count);
                NeedToStartBGM = true;
                if (bgmThread == null)
                {
                    bgmThread = new Thread(new ThreadStart(playMusicInBackground));
                    bgmThread.Name = "BGM Thread";
                    bgmThread.Start();
                }
            }
        }
        private void playMusicInBackground()
        {
            Song bgm = null;
            while (true)
            {
                if (NeedToStartBGM)
                {
                    MediaPlayer.Volume = MediaVolume;
                    bgm = Content.Load<Song>(PREFIX + "bgm/" + bgms[CurrentBGM]);
                    MediaPlayer.Play(bgm);
                    MediaPlayer.IsRepeating = true;
                    NeedToStartBGM = false;
                }
                else if (NeedToStopBGM)
                {
                    if (MediaPlayer.Volume > 0.0F)
                        MediaPlayer.Volume -= MediaVolume / 10F;
                    else
                    {
                        MediaPlayer.Stop();
                        NeedToStopBGM = false;
                    }
                }

                if (NeedToExitBGM)
                    break;

                Thread.Sleep(100);
            }
        }
        private void playSound(SFX which, bool unique)
        {
            //TODO there have got to be half a dozen cases where this doesn't work right...
            if (unique)
            {
                SoundEffectInstance cur;
                if (playing.TryGetValue(which, out cur))
                {
                    cur.Stop();
                    cur.Play();
                    return;
                }
                SoundEffectInstance sei = getSound(which).CreateInstance();
                playing.Add(which, sei);
                sei.Play();
            }
            else
            {
                SoundEffect s = getSound(which);
                if (s != null)
                {
                    s.Play();
                }
            }

        }

        private SoundEffect getSound(SFX which)
        {
            SoundEffect s;
            if (!instance.sounds.TryGetValue(which, out s)) //haven't loaded the sounds yet
            {
                try
                {
                    string name = names[which];
                    s = instance.Content.Load<SoundEffect>(PREFIX + name);
                }
                catch (Exception  ex)
                {
                    trace("Couldn't load " + which.ToString() + ": " + ex);
                }
                
                sounds.Add(which, s);
            }
            
            return s;
        }
        private void trace(string s)
        {
            Console.WriteLine("SoundManager: " + s);
        }

        internal static void StopBGM()
        {
            instance.stopBGM();
        }

        private void stopBGM()
        {
            NeedToStopBGM = true;
        }

       
    }
}
