#undef USE_STICK_AS_CURSOR 

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


namespace ChromathudWin
{
    /// <summary>
    /// Types of controls for the game independent of the keys used
    /// </summary>
    public enum UserCommand
    {
        None,

        //Navigating menus
        MenuDown,
        MenuUp,
        MenuLeft,
        MenuRight,
        MenuArm,
        MenuFire,
        MenuSelect,
        MenuBack,
        MenuCursorMoved,
        MenuCursorMotionX,
        MenuCursorMotionY,
        MenuCursorX,
        MenuCursorY,
        MenuStart,

        //Gameplay
        GameQuit = 100,
        GameSelect,
        GameDeselect,
        GamePush,
        GameCheat,
        GamePause,
        GameUnpause,
        GameCursorX,
        GameCursorY,
        GameCursorUp = MenuUp,
        GameCursorDown = MenuDown,
        GameCursorLeft = MenuLeft,
        GameCursorRight = MenuRight,

        //Global
        ExitProgram = 200,
        ToggleTrialMode = 201,
        Any,

        //Misc
        SceneSkip = 300,
        SceneBack,
        SceneNext,

        NUM_USER_COMMANDS

    };

    /// <summary>
    /// This is the interface that should be used for all user input.
    /// It provides a central place to set controls and a layer where 
    /// game commands and other "functions" can be mapped 
    /// (possibly by the user) to one or more actual keys/buttons
    /// TODO less hard-coding, more mapping
    /// </summary>
    public class InputManager : GameComponent
    {
        private static InputManager instance;
        private static List<UserCommand> synthetics; //"synthetic" input created by PostEvent
        private static List<UserCommand> oldsynthetics;

#if XBOX
        private Vector2 fakeCursor;
#endif

        /// <summary>
        /// The active or primary player. On the Xbox, calls to GetXXX without
        /// specifying a PlayerIndex will check this controller
        /// </summary>
        //private PlayerIndex player;
        public static PlayerIndex ActivePlayer { get; set; }
        public static PlayerIndex SecondaryPlayer { get; set; }

        private InputManager(Game game)
        :
            base(game)
        {
            synthetics = new List<UserCommand>(0);
            oldsynthetics = new List<UserCommand>(0);
            Point c = Game.GraphicsDevice.Viewport.TitleSafeArea.Center;
#if XBOX 
            fakeCursor = new Vector2(c.X, c.Y);
#endif
        }

        public override void Initialize()
        {
            UpdateOrder = -1; //grab input at the beginning of each step
            base.Initialize();
        }
        /// <summary>
        /// Poll whether a certain user event was triggered, such as quitting
        /// </summary>
        /// <param name="cmd">The UserCommand to poll</param>
        /// <returns></returns>
        public static bool GetBoolean(UserCommand cmd)
        {
            //first look for synthetic input
            if (oldsynthetics.Contains(cmd))
                return true;

            return GetBoolean(cmd, ActivePlayer);
        }

        /// <summary>
        /// Poll whether a certain user event was triggered, such as quitting
        /// </summary>
        /// <param name="cmd">The UserCommand to poll</param>
        /// <param name="index">The player index, if applicable</param>
        /// <returns></returns>
        public static bool GetBoolean(UserCommand cmd, PlayerIndex index)
        {
            if (cmd == UserCommand.Any)
            {
                return (Keyboard.GetState().GetPressedKeys().Length > 0) ||
                        GamePadDelta.One.HasChanged() ||
                        GamePadDelta.Two.HasChanged() ||
                        GamePadDelta.Three.HasChanged() ||
                        GamePadDelta.Four.HasChanged();

            }
            if (cmd == UserCommand.ExitProgram)
            {
                return KeyboardDelta.WasKeyPressed(Keys.F10) ||
                    GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.BigButton);
            }
            if (cmd == UserCommand.ToggleTrialMode)
            {
                return GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.RightShoulder);
            }
            if (cmd == UserCommand.MenuStart)
            {
                return KeyboardDelta.WasKeyPressed(Keys.S) ||
                    GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.Start);
            }
            if (cmd == UserCommand.MenuUp)
            {
                return GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.DPadUp) ||
                    GamePadDelta.ForPlayer(index).WasStickTwitched(Stick.Left, .5F * Vector2.UnitY) ||
                    KeyboardDelta.WasKeyPressed(Keys.Up);
            }
            if (cmd == UserCommand.MenuDown)
            {
                return GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.DPadDown) ||
                    GamePadDelta.ForPlayer(index).WasStickTwitched(Stick.Left, -.5F * Vector2.UnitY) ||
                    KeyboardDelta.WasKeyPressed(Keys.Down);
            }
            if (cmd == UserCommand.MenuLeft)
            {
                return GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.DPadLeft) ||
                    GamePadDelta.ForPlayer(index).WasStickTwitched(Stick.Left, -.5F * Vector2.UnitX) ||
                    KeyboardDelta.WasKeyPressed(Keys.Left);
            }
            if (cmd == UserCommand.MenuRight)
            {
                return GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.DPadRight) ||
                    GamePadDelta.ForPlayer(index).WasStickTwitched(Stick.Left, .5F * Vector2.UnitX) ||
                    KeyboardDelta.WasKeyPressed(Keys.Right);
            }
            if (cmd == UserCommand.MenuBack)
            {
                return GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.Back) ||
                    GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.B) ||
                    KeyboardDelta.WasKeyPressed(Keys.Escape);
            }
            if (cmd == UserCommand.MenuArm)
            {
                return MouseDelta.GetState().WasLeftButtonPressed && ChromathudGame.Instance.IsActive ||
                       GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.A);
            }
            if (cmd == UserCommand.MenuFire)
            {
                return MouseDelta.GetState().WasLeftButtonReleased && ChromathudGame.Instance.IsActive ||
                    GamePadDelta.ForPlayer(index).WasButtonReleased(Buttons.A);
            }
            if (cmd == UserCommand.MenuSelect)
            {
                return GamePadDelta.ForPlayer(index).WasButtonReleased(Buttons.A) ||
                    KeyboardDelta.WasKeyReleased(Keys.Enter) ||
                    KeyboardDelta.WasKeyReleased(Keys.Space);
            }
            if (cmd == UserCommand.MenuCursorMoved)
            {
                return GetFloat(UserCommand.MenuCursorMotionX, index) + GetFloat(UserCommand.MenuCursorMotionY, index) != 0.0F;
            }
            if (cmd == UserCommand.GameQuit)
            {
                return KeyboardDelta.WasKeyPressed(Keys.Q);
            }
            if (cmd == UserCommand.GameSelect)
            {
                return GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.A) ||
                    GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.X) ||
                    MouseDelta.GetState().WasLeftButtonPressed && ChromathudGame.Instance.IsActive ||
                    KeyboardDelta.WasKeyPressed(Keys.Enter) ||
                    KeyboardDelta.WasKeyPressed(Keys.Z);
            }
            if (cmd == UserCommand.GameDeselect)
            {
                return GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.B) ||
                    GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.Y) ||
                    MouseDelta.GetState().WasRightButtonPressed && ChromathudGame.Instance.IsActive ||
                    KeyboardDelta.WasKeyPressed(Keys.Delete) ||
                    KeyboardDelta.WasKeyPressed(Keys.X);
            }
            if (cmd == UserCommand.GamePush)
            {
                return Keyboard.GetState().IsKeyDown(Keys.Space) ||
                    Mouse.GetState().MiddleButton == ButtonState.Pressed ||
                    GamePad.GetState(index).IsButtonDown(Buttons.LeftTrigger);
            }
            if (cmd == UserCommand.GameCheat)
            {
                return false;// KeyboardDelta.WasKeyPressed(Keys.C);
            }
            if (cmd == UserCommand.GamePause)
            {
                return GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.Start) ||
                    KeyboardDelta.WasKeyPressed(Keys.Escape) ||
                    KeyboardDelta.WasKeyPressed(Keys.P);
            }
            if (cmd == UserCommand.GameUnpause)
            {
                return GamePadDelta.ForPlayer(index).WasButtonPressed(Buttons.Start);
            }
            if (cmd == UserCommand.SceneNext)
            {
                return KeyboardDelta.WasKeyReleased(Keys.Enter) ||
                    KeyboardDelta.WasKeyReleased(Keys.Right) ||
                    KeyboardDelta.WasKeyReleased(Keys.Space) ||
                    GamePadDelta.ForPlayer(index).WasButtonReleased(Buttons.A) ||
                    MouseDelta.GetState().WasLeftButtonReleased && ChromathudGame.Instance.IsActive;
            }
            if (cmd == UserCommand.SceneBack)
            {
                return KeyboardDelta.WasKeyPressed(Keys.Left) ||
                    GamePadDelta.ForPlayer(index).WasButtonReleased(Buttons.Back) ||
                    GamePadDelta.ForPlayer(index).WasButtonReleased(Buttons.B) ||
                    MouseDelta.GetState().WasRightButtonReleased && ChromathudGame.Instance.IsActive;
            }
            if (cmd == UserCommand.SceneSkip)
            {
                return KeyboardDelta.WasKeyPressed(Keys.Escape) ||
                    GamePadDelta.ForPlayer(index).WasButtonReleased(Buttons.Start);
            }
            return false;
        }
        public static InputManager GetInstance(Game game)
        {
            if (instance == null)
                instance = new InputManager(game);
            return instance;
        }
        public static float GetFloat(UserCommand cmd)
        {
            return GetFloat(cmd, PlayerIndex.One);
        }
        public static float GetFloat(UserCommand cmd, PlayerIndex index)
        {
            switch (cmd)
            {
#if XBOX
                case UserCommand.MenuCursorMotionX: return GamePad.GetState(index).ThumbSticks.Left.X;
                case UserCommand.MenuCursorMotionY: return GamePad.GetState(index).ThumbSticks.Left.Y;
                case UserCommand.MenuCursorX:
                case UserCommand.GameCursorX: return instance.fakeCursor.X;
                case UserCommand.MenuCursorY:
                case UserCommand.GameCursorY: return instance.fakeCursor.Y;
#else
                case UserCommand.MenuCursorMotionX: return MouseDelta.GetState().X;
                case UserCommand.MenuCursorMotionY: return MouseDelta.GetState().Y;
                case UserCommand.MenuCursorX:
                case UserCommand.GameCursorX: return Mouse.GetState().X;
                case UserCommand.MenuCursorY:
                case UserCommand.GameCursorY: return Mouse.GetState().Y;
#endif
            }
            return 0.0F;
        }
        /// <summary>
        /// Manually generate a UserCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int PostEvent(UserCommand cmd, PlayerIndex index)
        {
            synthetics.Add(cmd);
            return 0;
        }
        /// <summary>
        /// Instantly set the position of the cursor
        /// </summary>
        /// <param name="x">X coordinate to warp to</param>
        /// <param name="y">Y coordinate to warp to</param>
        /// <returns>Zero.</returns>
        public static int WarpCursor(int x, int y)
        {
            return WarpCursor(x, y, false);
        }
        public static int WarpCursor(int x, int y, bool relative)
        {
#if XBOX
            if (relative)
            {
                instance.fakeCursor.X += x;
                instance.fakeCursor.Y += y;
            }
            else
            {
                instance.fakeCursor.X = x;
                instance.fakeCursor.Y = y;
            }
#elif WINDOWS
            if (!relative)
                Mouse.SetPosition(x, y);
            else
                Mouse.SetPosition(Mouse.GetState().X + x, Mouse.GetState().Y + y);
#endif
            return 0;
        }

        public override void Update(GameTime gameTime)
        {
            //incorporate any synthetics posted last frame
            oldsynthetics.Clear();
            foreach (UserCommand uc in synthetics)
            {
                oldsynthetics.Add(uc);
            }
            synthetics.Clear();
         
            //see what changed
            KeyboardDelta.Update(gameTime);
            GamePadDelta.UpdateAll(gameTime);
            MouseDelta.GetState().Update(gameTime);

#if XBOX && USE_STICK_AS_CURSOR
                
            fakeCursor += GamePad.GetState(index).ThumbSticks.Left;
            if (fakeCursor.X < 0)
                fakeCursor.X = 0;
            if (fakeCursor.Y < 0)
                fakeCursor.Y = 0;
            
#endif
            base.Update(gameTime);
        }
    }
}