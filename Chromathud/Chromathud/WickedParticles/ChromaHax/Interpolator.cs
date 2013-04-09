using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WickedLibrary.Graphics.Hax
{
    public class Interpolator
    {
        internal static float LinearInterp(float p, float p_2, float percent)
        {
            return p_2 * percent + p * (1 - percent);
        }
        internal static byte LinearInterp(byte p, byte p_2, float percent)
        {
            return (byte)(p_2 * percent + p * (1 - percent));
        }
    }
}