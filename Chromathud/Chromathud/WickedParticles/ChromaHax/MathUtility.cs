using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WickedLibrary.Graphics.Hax
{
    public class MathUtility
    {
        public static float Epsilon = .0000000000000000000001F;

        public static Random Random
        {
            get 
            {
                if (random == null)
                    random = new Random();
                return random;
            }
            set { random = value; }
        }
        private static Random random;


        internal static Microsoft.Xna.Framework.Vector2 ToCartesian(float placeAngle, float p)
        {
            throw new NotImplementedException();
        }

        internal static float RandomFloat(float MinParticleLongevity, float MaxParticleLongevity)
        {
            throw new NotImplementedException();
        }
    }
}