using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;


namespace ChromathudWin
{
    /// <summary>
    /// Enhances the MouseState interface by tracking what has changed
    /// </summary>
    class MouseDelta
    {
        private static MouseDelta instance;
        private MouseState oldState;
        private MouseState newState;

        /// <summary>
        /// Horizontal movement of the mouse since the last update
        /// </summary>
        public int X
        {
            get { return newState.X - oldState.X; }
        }
        /// <summary>
        /// Vertical movement of the mouse since the last update
        /// </summary>
        public int Y
        {
            get { return newState.Y - oldState.Y; }
        }
        /// <summary>
        /// Has the left mouse button been clicked since the last update
        /// </summary>
        public bool WasLeftButtonPressed
        {
            get
            {
                return (newState.LeftButton == ButtonState.Pressed) &&
                (oldState.LeftButton != ButtonState.Pressed);
            }
        }
        /// <summary>
        /// Has the left mouse button been released since the last update
        /// </summary>
        public bool WasLeftButtonReleased
        {
            get
            {
                return (newState.LeftButton == ButtonState.Released) &&
                (oldState.LeftButton != ButtonState.Released);
            }
        }
        /// <summary>
        /// Has the right mouse button been clicked since the last update
        /// </summary>
        public bool WasRightButtonPressed
        {
            get
            {
                return (newState.RightButton == ButtonState.Pressed) &&
                (oldState.RightButton != ButtonState.Pressed);
            }
        }
        /// <summary>
        /// Has the right mouse button been released since the last update
        /// </summary>
        public bool WasRightButtonReleased
        {
            get
            {
                return (newState.RightButton == ButtonState.Released) &&
                (oldState.RightButton != ButtonState.Released);
            }
        }
        /// <summary>
        /// How many clicks the wheel was scrolled since the last update
        /// </summary>
        public int ScrollWheelValue
        {
            get { return newState.ScrollWheelValue - oldState.ScrollWheelValue; }
        }
        /// <summary>
        /// Poll new input
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            oldState = newState;
            newState = Mouse.GetState();
        }
        /// <summary>
        /// Get the MouseDelta instance
        /// </summary>
        /// <returns>the instance</returns>
        public static MouseDelta GetState()
        {
            if (instance == null)
                instance = new MouseDelta();
            return instance;
        }
    }
}
