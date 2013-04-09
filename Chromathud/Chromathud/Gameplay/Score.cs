using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Serialization;
using EasyStorage;
using Microsoft.Xna.Framework;

namespace ChromathudWin.Gameplay
{
    public class Score
    {
        private UInt32 total;
        private string player;
        private DateTime date;
        private bool changed;

    
        public struct Entry
        {
            public uint score;
            public string name;
            public override string ToString()
            {
                return score + ": " + name;
            }
        }


        public bool Changed
        {
            get { return changed; }
            set { changed = value; }
        }

        public int LeaderboardRank { get; private set; }

        private List<Entry> top10 = null;

        /// <summary>
        /// How many scores are saved
        /// </summary>
        public const int HIGH_SCORES_SAVED = 10;

        /// <summary>
        /// The current game mode
        /// </summary>
        public string Mode { get; set; }
        public uint LastBonus { get; set; }

        public UInt32 Total
        {
            get { return total; }
#if DEBUG
            set
            {
                if (Preferences.GetBoolean("DebugMode"))
                    total = value;
            }
#endif
        }
        public UInt32 VisibleTotal
        {
            get { return GetVisibleScore((UInt32)total); }
        }
        /// <summary>
        /// When displaying score externally, multiply it by 10, because round numbers are cool and stuff.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static uint GetVisibleScore(uint p)
        {
            return p * 10;
        }
        /// <summary>
        /// 
        /// </summary>
        public Score()
        {
            total = 0;
            date = DateTime.Now;
            player = "Player";
            LeaderboardRank = -1;
        }
        /// <summary>
        /// Calculate score for clearing a group
        /// </summary>
        /// <param name="size">How many blocks were cleared</param>
        /// <param name="type">The type of block</param>
        public void ClearedGroup(int size, int type)
        {
            LastBonus = (UInt32)((size - 3) * (size - 3) * Math.Max(Math.Abs(type - 5), 1));
            total += LastBonus;
        }
        /// <summary>
        /// Calculate score for hitting the target sum
        /// </summary>
        /// <param name="num">Number of blocks used</param>
        /// <param name="avail">The select limit</param>
        /// <param name="millis">Milliseconds taken to solve</param>
        public void TargetReached(int num, int avail, int millis)
        {
            //multiplier for quick solving
            int tier =
                millis < 15000 ? 4 :
                millis < 30000 ? 3 :
                millis < 60000 ? 2 :
                1;

            int multiplier = 2;

            Debug.Assert(num != 0);
            LastBonus = (uint)((1 + avail - num) * tier * multiplier);
            total += LastBonus;
        }

        private List<Entry> checkLeaderboard(IAsyncSaveDevice device, out bool newScoreDidPlace)
        {

            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(List<Entry>));

            try
            {
                device.Load(ChromathudGame.Name, hsFileName(), stream =>
                {
                    top10 = new List<Entry>((List<Entry>)ser.Deserialize(stream));
                });
          
            }
            catch (Exception e)
            {
                top10 = new List<Entry>();
            }
  
            newScoreDidPlace = processLeaderboard();

            return top10;
        }

        private bool processLeaderboard()
        {
            bool changed = false;
            //add the score if it beats existing scores or if there aren't yet enough
            for (int i = 0; i < HIGH_SCORES_SAVED; ++i)
            {
                if (i >= top10.Count)
                {
                    top10.Add(new Entry { score = VisibleTotal, name = "" });
                    changed = true;
                    LeaderboardRank = i;
                    break;
                }
                if (VisibleTotal >= top10[i].score)
                {
                    top10.Insert(i, new Entry { score = VisibleTotal, name = "" });
                    if (top10.Count > HIGH_SCORES_SAVED)
                        top10.RemoveAt(top10.Count - 1);
                    changed = true;
                    LeaderboardRank = i;
                    break;
                }
            }
            return changed;
        }

        public List<Entry> CheckLeaderboard(IAsyncSaveDevice device)
        {
            return checkLeaderboard(device, out changed);
        }
        public List<Entry> CheckAndSaveLeaderboard(IAsyncSaveDevice device)
        {
            top10 = checkLeaderboard(device, out changed);
            return top10;
        }
        public bool SaveLeaderboard(IAsyncSaveDevice device)
        {
            return saveLeaderboard(device);
        }

        private bool saveLeaderboard(IAsyncSaveDevice device)
        {
            if (device.IsReady)
            {
                device.SaveCompleted += new SaveCompletedEventHandler((sender, args) =>
                {

                });
                device.SaveAsync(ChromathudGame.Name, hsFileName(), stream =>
                {
                    System.Xml.Serialization.XmlSerializer ser
                        = new System.Xml.Serialization.XmlSerializer(typeof(List<Entry>));
                    ser.Serialize(stream, top10);
                });
                return true;
            }
            return false;
        }

        private string hsFileName()
        {
            return Mode + "hiscores.xml";
        }

        public override string ToString()
        {
            return VisibleTotal.ToString();
        }
    }

}
