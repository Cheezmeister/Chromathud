using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WickedLibrary.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WickedLibrary.Graphics
{
    /// <summary>
    /// Container for basic particle properties
    /// </summary>
    public abstract class Particle : INode
    {
        public INode Next { get; set; }

        /// <summary>
        /// Whether this particle is still active or is dead
        /// </summary>
        public virtual bool Active
        {
            get { return age < maxAge; }
        }

        /// <summary>
        /// The age of the particle in seconds
        /// </summary>
        public float Age { get { return age; } }
        protected float age = 0.0f;

        /// <summary>
        /// The lifespan of the particle in seconds
        /// </summary>
        public float MaxAge
        {
            get { return maxAge; }
            set { maxAge = Math.Max(0, value); }
        }
        protected float maxAge = 1.0f;


        public virtual void Update(float elapsedTime)
        {
            age += elapsedTime;
        }

        /// <summary>
        /// Draws the particle using the given spritebatch. Assumes Begin has already been called.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void Draw2D(SpriteBatch spriteBatch) { }


        /// <summary>
        /// Resets the node to a neutral state. Should be overriden by subclasses to extend
        /// the reset.
        /// </summary>
        public virtual void Reset()
        {
            age = 0.0f;
            maxAge = 1.0f;
        }
    }
}