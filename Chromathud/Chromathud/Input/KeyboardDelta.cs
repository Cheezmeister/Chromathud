using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChromathudWin
{
    /// <summary>
    /// Enhances the KeyboardState interface by tracking what has changed
    /// </summary>
    class KeyboardDelta
    {
        static KeyboardState oldState;
        static KeyboardState newState;

        /// <summary>
        /// Poll whether a key was recently pressed
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool WasKeyPressed(Keys key)
        {
            return newState.IsKeyDown(key) && !oldState.IsKeyDown(key);
        }

        /// <summary>
        /// Poll whether a key was recently released
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool WasKeyReleased(Keys key)
        {
            return newState.IsKeyUp(key) && !oldState.IsKeyUp(key);
        }

        public static void Update(GameTime gameTime)
        {
            oldState = newState;
            newState = Keyboard.GetState();
        }
    }
}
