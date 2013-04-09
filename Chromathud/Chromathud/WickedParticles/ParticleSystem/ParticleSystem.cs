using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WickedLibrary.Collections;
//using WickedLibrary.Interfaces;

namespace WickedLibrary.Graphics
{
    public enum SpriteBlendMode
    {
        AlphaBlend
    }
    public enum SaveStateMode
    {
        None
    }

    public interface IParticleSystem : IUpdateable
    {
        /// <summary>
        /// Draws the particles in the system using the given sprite batch.
        /// Assumes it is being called between Begin() and End()
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="spriteBatch">The spritebatch to use for drawing</param>
        void Draw(GraphicsDevice graphics, SpriteBatch spriteBatch);

        /// <summary>
        /// Draws the particles in the system using an internal spritebatch
        /// </summary>
        /// <param name="graphics"></param>
        void Draw(GraphicsDevice graphics);
    }


    public class ParticleSystem<T> : IParticleSystem where T : Particle, new()
    {
        /// <summary>
        /// Blend mode to apply to drawing all particles in the system. Used when drawn with the internal spritebatch.
        /// </summary>
        public SpriteBlendMode BlendMode = SpriteBlendMode.AlphaBlend;

        /// <summary>
        /// The sort mode to apply to drawing all the particles in the system. Used when drawn with the internal spritebatch.
        /// </summary>
        public SpriteSortMode SortMode = SpriteSortMode.Texture;

        /// <summary>
        /// Transform to use when drawing all the particles in the system. Used when drawn with the internal spritebatch.
        /// </summary>
        public Matrix Transform = Matrix.Identity;

        /// <summary>
        /// Function that gets applied to each particle before its update
        /// </summary>
        public Action<T> PreUpdateHandler;

        //Particle pool
        private Pool<T> inactive;
        private T activeHead;
        private T activeTail;

        //Size of the pool
        private readonly int maxParticles;

        //Renderer
        private SpriteBatch spriteBatch;

        /// <summary>
        /// Creates a particle system with a pool of particles to use
        /// </summary>
        /// <param name="maxParticles"></param>
        public ParticleSystem(int maxParticles)
        {
            this.maxParticles = maxParticles;
            inactive = new Pool<T>(maxParticles);
            activeHead = activeTail = null;
        }

        /// <summary>
        /// Gets a particle from the pool. May return null if none are available.
        /// </summary>
        /// <returns></returns>
        internal T GetParticleFromPool()
        {
            return inactive.Get();
        }

        /// <summary>
        /// Adds a particle to the active list
        /// </summary>
        /// <param name="p"></param>
        internal void AddParticle(T p)
        {
            if (p == null) //If we run out of particles, we can exit now
                return;

            //Add new particle to active list
            p.Next = null;
            if (activeHead == null)
                activeHead = p;
            else
                activeTail.Next = p;
            activeTail = p;
        }


        /// <summary>
        /// Updates the particle system, updating every particle in it and 
        /// pooling any that have reached the end of their life
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            //elapsed time
            float eTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            T prev = null;
            T cur = activeHead;
            T next = null;

            while (cur != null)
            {
                next = (T)cur.Next;

                //If item is dead, return it to the pool
                if (!cur.Active)
                {
                    if (prev == null)//First item
                    {
                        //Return the item to the pool
                        inactive.Return(cur);

                        //Adjust our linkages to remove the item
                        activeHead = next;
                        prev = null;
                        cur = next;

                        if (next == null || next.Next == null)
                            activeTail = next;
                    }
                    else if (next == null) //Last item
                    {
                        //Return the item to the pool
                        inactive.Return(cur);

                        //We are done iterating
                        cur = null;

                        //Adjust our linkages to remove the item
                        activeTail = prev;
                        activeTail.Next = null;
                    }
                    else //Removing from middle
                    {
                        //Return the item to the pool
                        inactive.Return(cur);

                        //Adjust our linkages to remove the item
                        prev.Next = next;
                        cur = next;
                        next = (T)next.Next;
                    }
                }
                else
                {
                    //If particle is active, update it

                    if (PreUpdateHandler != null)
                        PreUpdateHandler(cur);
                    cur.Update(eTime);

                    prev = cur;
                    cur = next;
                }
            }
        }



        /// <summary>
        /// Draws the particles in the system using the given sprite batch.
        /// Assumes it is being called between Begin() and End()
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="spriteBatch">The spritebatch to use for drawing</param>
        public void Draw(GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            Particle p = activeHead;
            while (p != null)
            {
                p.Draw2D(spriteBatch);
                p = (T)p.Next;
            }
        }

        /// <summary>
        /// Draws the particles in the system using an internal spritebatch
        /// </summary>
        /// <param name="graphics"></param>
        public void Draw(GraphicsDevice graphics)
        {
            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(graphics);

            spriteBatch.Begin();
            Draw(graphics, spriteBatch);
            spriteBatch.End();
        }

        #region IUpdateable Members

        private bool enabled;

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public event EventHandler<EventArgs> EnabledChanged;

        public int UpdateOrder
        {
            get { return 0; }
        }

        public event EventHandler<EventArgs> UpdateOrderChanged;
        #endregion
    }
}
