using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChromathudWin
{
    /// <summary>
    /// These are position constants that are derived from the artwork and 
    /// must be hard-coded. Almost everything that appears on-screen is 
    /// affected by these numbers, and if they're incorrect, things begin
    /// to look funky. 
    /// 
    /// Vars marked FROM IMAGE are of most importance, since they're based
    /// directly on a certain image's dimensions, rather than guesswork.
    /// Lead artist should update these whenever changing artwork. 
    /// </summary>
    public class Layout
    {
#if XBOX 
        public const int BlockWidth = 36; //FROM IMAGE
#elif EMULATE_XBOX
        public const int BlockWidth = 36; //FROM IMAGE
#else
        public const int BlockWidth = 44;
#endif
        public const int FieldFrameThickness = 14; //FROM IMAGE

        //Center timer
        public const int TimerPadding = 20;

        //Selection indicator bar
#if XBOX || EMULATE_XBOX
        public const int SelectionBarLeftPad = 117; //FROM IMAGE
        public const int SelectionBarRightPad = 9; //FROM IMAGE
#else
        public const int SelectionBarLeftPad = 138; //FROM IMAGE
        public const int SelectionBarRightPad = 10; //FROM IMAGE
#endif


        //How far down from the top to show game notifications
        //This number is a percent of the field height, so e.g. 50 
        //puts them in the middle of the field
        public const int NotificationDepth = 25;

        //Separation between important areas
        public const int TopBuffer = 30;
        public const int BottomBuffer = TopBuffer;
        public const int LeftOuterBuffer = 35;
        public const int RightOuterBuffer = LeftOuterBuffer;

        //Baked-in text width
        public const int StatusUpperPadding = 0;
        public const int StatusIndentation = 0;
        public const int StatusVerticalSkew = 3;
        public const int StatusVerticalMargin = 7;
        public const int StatusHorizontalMargin = 7;
        public const int StatusSpacing = 55; //FROM IMAGE

        //First Player status
        public const int StatusOneWidth = 265; //FROM IMAGE
        public const int StatusOneHeight = StatusOneWidth;

        //Second Player status
        public const int StatusTwoWidth = StatusOneWidth;
        public const int StatusTwoHeight = StatusOneHeight;

        //Menus
        public const int ButtonsPerColumn = 4;
        public const int ButtonWidth = 340; //FROM IMAGE
        public const int ButtonPadX = 11; //FROM IMAGE
        public const int ButtonHeight = 90; //FROM IMAGE
        public const int ButtonPadY = 12; //FROM IMAGE
    }
}
