using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WickedLibrary.Graphics.Hax
{
    public class TextureData
    {
        private Texture2D texture;
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        private Vector2 origin;

        public Vector2 Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        internal static Microsoft.Xna.Framework.Vector2 GetOrigin(object p, object p_2, RegistrationLocation reg)
        {
            throw new NotImplementedException();
        }
    }
}