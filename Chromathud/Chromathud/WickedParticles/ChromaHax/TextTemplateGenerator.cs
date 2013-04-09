using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WickedLibrary.Graphics.Hax
{
    public class TextTemplateGenerator
    {
        private int width;

        public int Width
        {
            get { return width; }
            set { width = value; }
        }
        private int height;

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        internal Vector2 SelectRandom()
        {
            throw new NotImplementedException();
        }
    }
}