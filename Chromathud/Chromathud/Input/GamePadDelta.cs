using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChromathudWin
{
    enum Stick
    {
        Left,
        Right,
    }

    /// <summary>
    /// Enhances the GamePadState interface by tracking what has changed
    /// </summary>
    class GamePadDelta
    {
        GamePadState oldState;
        GamePadState newState;
        PlayerIndex index;

        private static GamePadDelta one   = new GamePadDelta(PlayerIndex.One);
        private static GamePadDelta two   = new GamePadDelta(PlayerIndex.Two);
        private static GamePadDelta three = new GamePadDelta(PlayerIndex.Three);
        private static GamePadDelta four  = new GamePadDelta(PlayerIndex.Four);
        public static Indexer Pad { get; private set; }

        #region properties
        public static GamePadDelta One
        {
            get { return one; }
        }
        public static GamePadDelta Two
        {
            get { return two; }
        }
        public static GamePadDelta Three
        {
            get { return three; }
        }
        public static GamePadDelta Four
        {
            get { return four; }
        }
        public bool WasDisconnected
        {
            get
            {
                return oldState.IsConnected && !newState.IsConnected;
            }
        }
        public bool WasConnected
        {
            get
            {
                return !oldState.IsConnected && newState.IsConnected;
            }
        }
        #endregion

        private GamePadDelta(PlayerIndex index)
        {
            this.index = index;
        }

        /// <summary>
        /// Get the GamePadDelta object concerned with a certain player
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static GamePadDelta ForPlayer(PlayerIndex index)
        {
            return index == PlayerIndex.One ? One :
                index == PlayerIndex.Two ? Two :
                index == PlayerIndex.Three ? Three :
                index == PlayerIndex.Four ? Four :
                null;
        }
        /// <summary>
        /// See if the pad state has changed at all
        /// </summary>
        /// <returns></returns>
        public bool HasChanged()
        {
            return oldState.Buttons != newState.Buttons ||
                oldState.DPad != newState.DPad ||
                oldState.ThumbSticks != newState.ThumbSticks ||
                oldState.Triggers != newState.Triggers;
        }

        /// <summary>
        /// Checks if a button has been pressed (that is, has gone from up to down)
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool WasButtonPressed(Buttons button)
        {
            return newState.IsButtonDown(button) && !oldState.IsButtonDown(button);
        }

        /// <summary>
        /// Checks if a button has been released (that is, has gone from down to up)
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool WasButtonReleased(Buttons button)
        {
            return newState.IsButtonUp(button) && !oldState.IsButtonUp(button);
        }

        /// <summary>
        /// Checks if a thumbstick was quickly tapped in a certain direction. 
        /// This is useful for navigating menus and other situations where
        /// we treat a thumbstick as a D-Pad.
        /// </summary>
        /// <param name="which">Which stick to check: left or right</param>
        /// <param name="direction">A vector in the direction to check. 
        /// The length, which should be between 0.0 and 1.0, determines how
        /// far the stick must be rocked to "count"</param>
        /// <returns></returns>
        public bool WasStickTwitched(Stick which, Vector2 direction)
        {
            if (direction.X == 0 && direction.Y == 0)
                return false;

            Vector2 stickOld, stickNew;
            if (which == Stick.Left)
            {
                stickOld = oldState.ThumbSticks.Left;
                stickNew = newState.ThumbSticks.Left;
            }
            else
            {
                stickOld = oldState.ThumbSticks.Right;
                stickNew = newState.ThumbSticks.Right;
            }

            Vector2 twitch = stickNew;
            bool x = (direction.X == 0 || twitch.X / direction.X > 1);
            bool y = (direction.Y == 0 || twitch.Y / direction.Y > 1);
            bool twitchNew = x && y;

            twitch = stickOld;
            x      = (direction.X == 0 || twitch.X / direction.X > 1);
            y      = (direction.Y == 0 || twitch.Y / direction.Y > 1);
            bool twitchOld = x && y;

            return twitchNew && !twitchOld;
        }

        /// <summary>
        /// A GamePadThumbSticks structure with fields that represent the 
        /// change in the positions of the thumb sticks
        /// </summary>
        public GamePadThumbSticks ThumbSticks
        {
            get
            {
                GamePadThumbSticks ret = new GamePadThumbSticks(
                    newState.ThumbSticks.Left - oldState.ThumbSticks.Left,
                    newState.ThumbSticks.Right - oldState.ThumbSticks.Right);
                return ret;
            }
        }

        /// <summary>
        /// Grab new input
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            oldState = newState;
            newState = GamePad.GetState(index);
        }

        /// <summary>
        /// Update all four GamePadDeltas
        /// </summary>
        /// <param name="gameTime"></param>
        public static void UpdateAll(GameTime gameTime)
        {
            one.Update(gameTime);
            two.Update(gameTime);
            three.Update(gameTime);
            four.Update(gameTime);
        }

        public class Indexer
        {
            public GamePadDelta this[PlayerIndex index]
            {
                get
                {
                    return GamePadDelta.ForPlayer(index);
                }
            }
        }

    }
}
