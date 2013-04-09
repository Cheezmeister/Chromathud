using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;

using System.Text.RegularExpressions;
using System.Security.Principal;


namespace ChromathudWin
{
    /// <summary>
    /// Miscellaneous utilities
    /// </summary>
    static class Util
    {
        /// <summary>
        /// A global font used by text drawing extension methods
        /// This member must be initialized by calling code before using text extension methods
        /// </summary>
        public static SpriteFont GlobalFont
        {
            get { return Util.globalFont; }
            set { Util.globalFont = value; }
        }
        private static SpriteFont globalFont;


        /// <summary>
        /// A Texture2D that is a single black pixel. Useful for drawing primitives, etc.
        /// This member must be initialized by calling code before using prim extension methods
        /// </summary>
        public static Texture2D Pixel
        {
            get { return Util.pixel; }
            set { Util.pixel = value; }
        }
        private static Texture2D pixel;

        public delegate void Subroutine<T>(T item);
        /// <summary>
        /// Mimic a foreach loop WITHOUT FREAKING CRASHING on modification of collection
        /// </summary>
        /// <typeparam name="T">The parameter of the collection</typeparam>
        /// <param name="collection">The collection to iterate</param>
        /// <param name="operate">The operation to perform each iteration</param>
        /// <example>
        /// Foreach&ltint&rt(PhoneNumbers, num => { 
        ///     // We don't like Jenny.
        ///     if (num == 8674309) PhoneNumbers.Remove(num); 
        /// });
        /// </example>
        public static void Foreach<T>(ICollection<T> collection, Subroutine<T> operate)
        {
            List<T> temp = new List<T>(collection);
            foreach (T item in temp)
            {
                operate(item);
            }
        }
        /// <summary>
        /// Grab the top left corner of a rectangle
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Vector2 TopLeftCorner(Rectangle r)
        {
            return new Vector2(r.Left, r.Top);
        }
        /// <summary>
        /// Given a Viewport, create a Rectangle representing its area
        /// </summary>
        /// <param name="v">The Viewport</param>
        /// <returns>The Viewport's area as a Rectangle</returns>
        public static Rectangle GetRect(this Microsoft.Xna.Framework.Graphics.Viewport v)
        {
            Rectangle rect = new Rectangle(v.X, v.Y, v.Width, v.Height);
            return rect;
        }

        /// <summary>
        /// Scale a sprite to fit in the given rectangle. 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="rect"></param>
        /// <param name="horizontalAlign"></param>
        /// <param name="verticalAlign"></param>
        /// <returns></returns>
        public static Rectangle FitIn(this Texture2D item, Rectangle dest, short horizontalAlign, short verticalAlign)
        {
            double xscale = (double)item.Width / (double)dest.Width;
            double yscale = (double)item.Height / (double)dest.Height;

            // sprite smaller than rectangle--just align it
            if (xscale <= 1 && yscale <= 1)
                return new Rectangle(
                    dest.Left + (dest.Width - item.Width) / 2 * (horizontalAlign + 1),
                    dest.Top + (dest.Height - item.Height) / 2 * (verticalAlign + 1), 
                    item.Width, 
                    item.Height);
 
            // sprite bigger than rectangle--squeeze it
            if (xscale > yscale)
                return Util.Scale(dest, 1, yscale / xscale, horizontalAlign, verticalAlign);
            if (xscale < yscale)
                return Util.Scale(dest, xscale / yscale, 1, horizontalAlign, verticalAlign);

            return dest;
        }
        /// <summary>
        /// Center a sprite in (or about) the given rectangle
        /// </summary>
        /// <param name="sprite">The sprite to draw</param>
        /// <param name="rect">A screen space rectangle</param>
        /// <returns>The draw area</returns>
        public static Rectangle CenterIn(this Texture2D sprite, Rectangle rect)
        {
            Rectangle ret = new Rectangle();
            ret.X = rect.X + (rect.Width - sprite.Width) / 2;
            ret.Y = rect.Y + (rect.Height - sprite.Height) / 2;
            ret.Width = sprite.Width;
            ret.Height = sprite.Height;
            return ret;
        }
        /// <summary>
        /// Take the max of many numbers
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double Max(params double[] c)
        {
            double max = double.MinValue;
            foreach (double d in c)
            {
                max = Math.Max(d, max);
            }
            return max;
        }
        /// <summary>
        /// Take the min of many numbers
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double Min(params double[] c)
        {
            double max = double.MaxValue;
            foreach (double d in c)
            {
                max = Math.Min(d, max);
            }
            return max;
        }

        private static Random rand;
        public static Random Random
        {
            get
            {
                if (rand == null)
                    rand = new Random();
                return rand;
            }
        }

        #region Extension methods
        public static float NextFloat(this Random rand)
        {
            return (float)rand.NextDouble();
        }
        public static string ImageDir = "art";
        public static Texture2D LoadImage(this ContentManager content, string assetName)
        {
            return content.Load<Texture2D>(ImageDir + System.IO.Path.DirectorySeparatorChar + assetName);
        }
        public static SpriteFont LoadFont(this ContentManager content, string assetName)
        {
            return content.Load<SpriteFont>(FontDir + System.IO.Path.DirectorySeparatorChar + assetName);
        }
        public static Rectangle Scale(this Rectangle rect, double xScale, double yScale, short horizontalAlign, short verticalAlign)
        {
            Point center = rect.Center;
            int right = rect.Right;
            int bottom = rect.Bottom;

            double width = (double)rect.Width * xScale;
            double height = (double)rect.Height * yScale;

            rect.Width = (int)width;
            rect.Height = (int)height;

            if (horizontalAlign == 0)
                rect.X += (right - rect.Right) / 2;
            else if (horizontalAlign > 0)
                rect.X += (right - rect.Right);

            if (verticalAlign == 0)
                rect.Y += (bottom - rect.Bottom) / 2;
            else if (verticalAlign > 0)
                rect.Y += (bottom - rect.Bottom) / 2;

            return rect;
        }

        public static string FontDir = "fonts";
        #region SpriteBatch extension methods
        public static void DrawTextScaled(this SpriteBatch spriteBatch, Rectangle location, string text, int horizontalAlign, int verticalAlign)
        {
            DrawTextScaled(spriteBatch, location, text, horizontalAlign, verticalAlign, Color.White);
        }

        public static void DrawTextScaled(this SpriteBatch SpriteBatch, Rectangle location, string text, int horizontalAlign, int verticalAlign, Color color)
        {
            DrawTextScaled(SpriteBatch, location, text, horizontalAlign, verticalAlign, color, GlobalFont);
        }
        /// <summary>
        /// Draw text and scale it to fit inside a bounding box. The text
        /// will be scaled uniformly to fill the height or width of the 
        /// given box, whichever is smaller
        /// </summary>
        /// <param name="location">The bounding box for the text</param>
        /// <param name="text">The string to draw</param>
        /// <param name="horizontalAlign">Text will be left-aligned if negative, centered if zero, and right-aligned if positive</param>
        /// <param name="verticalAlign">Text will be top-aligned if negative, centered if zero, and bottom-aligned if positive</param>
        /// <param name="color">The color to use, defaults to Color.White</param>
        /// <param name="font">The font to use, defaults to Util.GlobalFont</param>
        public static void DrawTextScaled(this SpriteBatch SpriteBatch, Rectangle location, string text, int horizontalAlign, int verticalAlign, Color color, SpriteFont font)
        {
            //find out how large the text is normally rendered
            Vector2 rawSize = font.MeasureString(text);
            float length = rawSize.X;
            float height = rawSize.Y;

            //fit to the given rect
            float scale = location.Width / length;
            if (height * scale > location.Height)
                scale = location.Height / height;

            height *= scale;
            length *= scale;

            Point p = location.Center;
            Vector2 origin = new Vector2();
            Vector2 position = new Vector2();

            if (horizontalAlign < 0)
            {
                position.X = location.Left;
            }
            else if (horizontalAlign > 0)
            {
                origin.X = rawSize.X;
                position.X = location.Right;
            }
            else
            {
                origin.X = rawSize.X / 2;
                position.X = p.X;
            }
            if (verticalAlign < 0)
            {
                position.Y = location.Top;
            }
            else if (verticalAlign > 0)
            {
                origin.Y = rawSize.Y;
                position.Y = location.Bottom;
            }
            else
            {
                origin.Y = rawSize.Y / 2;
                position.Y = p.Y;
            }

            if (Preferences.GetBoolean("DebugMode"))
            {
                Color c = Util.MakeTranslucent(Color.Black, 30);
                DrawRectangle(SpriteBatch, location, c);
            }
            SpriteBatch.DrawString(font, text, position, color, 0, origin, scale, SpriteEffects.None, 0.5F);

        }
        public static void DrawText(this SpriteBatch SpriteBatch, int x, int y, string text)
        {
            DrawText(SpriteBatch, new Vector2(x, y), text);
        }
        public static void DrawText(this SpriteBatch SpriteBatch, int x, int y, string text, Color color)
        {
            DrawText(SpriteBatch, new Vector2(x, y), text, color);
        }
        public static void DrawText(this SpriteBatch SpriteBatch, Vector2 location, string text)
        {
            DrawText(SpriteBatch, location, text, Color.Black);
        }
        public static void DrawText(this SpriteBatch SpriteBatch, Vector2 location, string text, Color color)
        {
            DrawText(SpriteBatch, location, text, color, 0, Vector2.Zero, 1, 0, SpriteEffects.None);
        }
        public static void DrawText(this SpriteBatch SpriteBatch, Vector2 location, string text, Color color, float layerDepth, Vector2 origin, float scale, float rotation, SpriteEffects effects)
        {
            SpriteBatch.DrawString(globalFont, text, location, color, rotation, origin, scale, SpriteEffects.None, layerDepth);
        }
        public static void DrawRectangle(this SpriteBatch SpriteBatch, Rectangle rect, Color color)
        {
            if (pixel == null)
                throw new Exception("Util.Pixel must be set before calling DrawRectangle");
            SpriteBatch.Draw(Pixel, rect, color);
        }
        /// <summary>
        /// Replace mnemonics in a string with Chromathud's bitmap font button characters
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SubstituteButtonChars(string s)
        {
            //FIXME our bitmap font is missing these
            s = Regex.Replace(s, "{A}", "\x80");
            s = Regex.Replace(s, "{B}", "\x81");
            s = Regex.Replace(s, "{X}", "\x82");
            s = Regex.Replace(s, "{Y}", "\x83");
            s = Regex.Replace(s, "{R}", "\x84");
            s = Regex.Replace(s, "{L}", "\x85");
            s = Regex.Replace(s, "{D}", "\x86");
            s = Regex.Replace(s, "{U}", "\x87");
            s = Regex.Replace(s, "{<}", "\x88");
            s = Regex.Replace(s, "{>}", "\x89");
            s = Regex.Replace(s, "{LT}", "\x8a");
            s = Regex.Replace(s, "{RT}", "\x8b");
            s = Regex.Replace(s, "{LB}", "\x8c");
            s = Regex.Replace(s, "{RB}", "\x8d");
            s = Regex.Replace(s, "{LS}", "\x8e");
            s = Regex.Replace(s, "{RS}", "\x8f");
            return s;
        }
        #endregion
        public static Color MakeTranslucent(this Color color, byte alpha)
        {
            return color * (alpha / 255.0f);
        }
        #region .NET replacements for Xbox
        public static void RemoveAll<T>(this List<T> list, Predicate<T> predicate)
        {
            list.ForEach(x =>
                {
                    if (predicate(x))
                        list.Remove(x);
                });
        }
        public static bool Exists<T>(this List<T> list, Predicate<T> predicate)
        {
            foreach (T t in list)
                if (predicate(t))
                    return true;
            return false;
        }
        #endregion
        #endregion
        /// <summary>
        /// Split a string into multiple lines so that, when rendered, 
        /// it approximates the desired aspect ratio. 
        /// 
        /// Works best with larger ratios or larger strings.
        /// 
        /// Not very fancy.
        /// </summary>
        /// <param name="text">The string to split</param>
        /// <param name="targetAspect">Aspect to match (width / height)</param>
        /// <returns>A multi-line string</returns>
        public static String WrapToAspect(String text, float targetAspect)
        {
            //find out how large the text is normally rendered
            Vector2 rawSize = globalFont.MeasureString(text);
            double length = rawSize.X;
            double height = rawSize.Y;
            double aspect = length / height;
            StringBuilder sb = new StringBuilder(text);
 
            double n = Math.Sqrt(aspect / targetAspect);
            int numLines = (int)Math.Floor(n);

            int lineWidth = sb.Length / numLines;
            
            return WrapToLength(text, lineWidth);
        }

        public static String WrapToLength(String text, int length)
        {
            StringBuilder sb = new StringBuilder(text);

            int lineWidth = length;

            for (int i = lineWidth, col = lineWidth; i < sb.Length; ++i, ++col)
            {
                if (sb[i] == '\t' || sb[i] == ' ')
                    sb[i] = '\n';

                // If we split (or a newline was already there), do next line
                if (sb[i] == '\n')
                {
                    i += lineWidth;
                    col = lineWidth;
                }
            }
            return sb.ToString();
        }

        public static Rectangle FindRectInMask(Texture2D mask, Color color)
        {
            Rectangle[] ret = FindRectsInMask(mask, color);
            if (ret == null) throw new Exception(" mask not found");

            return ret[0];
        }
        public static Rectangle[] FindRectsInMask(Texture2D mask, params Color[] colors)
        {
            Color[] data = new Color[mask.Width * mask.Height];
            List<Rectangle> ret = new List<Rectangle>();
            foreach (Color c in colors)
            {
                mask.GetData<Color>(data);
                Rectangle rect = new Rectangle(-1, -1, -1, -1);
                for (int i = 0; i < mask.Width; ++i)
                {
                    for (int j = 0; j < mask.Height; ++j)
                    {
                        Color temp = data[j * mask.Width + i];
                        if (temp.Equals(c))
                        {
                            if (rect.X == -1)
                                rect.X = i;
                            if (rect.Y == -1)
                                rect.Y = j;

                            rect.Width = i - rect.X + 1;
                            rect.Height = j - rect.Y + 1;
                        }
                    }
                }

                if (rect.X > -1 && rect.Y > -1)
                {
                    ret.Add(rect);
                }
            }
            return ret.ToArray();
        }

        public static bool UseXboxUI()
        {
#if XBOX
            return true;
#elif EMULATE_XBOX
            return true;
#else
            return false;
#endif
        }
        public static bool UseWindowsUI()
        {
            return !UseXboxUI();
        }
        public static TimeSpan Max(TimeSpan a, TimeSpan b)
        {
            return a > b ? a : b;
        }
        public static TimeSpan Min(TimeSpan a, TimeSpan b)
        {
            return a < b ? a : b;
        }

        public static String GetPlayerName(PlayerIndex index)
        {
#if XBOX
            try
            {
                return Gamer.SignedInGamers[index].Gamertag;
            }
            catch (Exception)
            {
                return "Player " + index;
            }
#else
            return WindowsIdentity.GetCurrent().Name.Split('\\')[1];
#endif
        }

        public static bool IsAlmostEaster()
        {
            DateTime today = DateTime.Today;
            double d = (EasterSunday(today.Year) - today).TotalDays;
            return (d < 7 && d > 0);
        }
        public static DateTime EasterSunday(int year)
        {
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }

    }
}
