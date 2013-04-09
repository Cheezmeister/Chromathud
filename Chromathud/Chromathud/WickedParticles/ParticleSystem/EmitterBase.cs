using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WickedLibrary.Graphics
{
    public interface IEmitter
    {
        void Update(GameTime gameTime);
        void GenerateParticle();
    }

    /// <summary>
    /// Base for all emitter objects. Contains only wrapper functionality
    /// for generating particles.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EmitterBase<T> : IEmitter where T : Particle, new()
    {
        protected ParticleSystem<T> pSystem;

        /// <summary>
        /// Creates an emitter that emits particles into a given particle system
        /// </summary>
        /// <param name="particleSystem"></param>
        protected EmitterBase(ParticleSystem<T> particleSystem)
        {
            this.pSystem = particleSystem;
        }

        /// <summary>
        /// Performs any per-frame behavior, such as moving and creating particles
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);

        /// <summary>
        /// Used to generate a particle in the attached particle system.
        /// May fail to do anything if no unused particles are available in the system.
        /// </summary>
        public void GenerateParticle()
        {
            T particle = pSystem.GetParticleFromPool();
            if (particle == null) //No need to go further if no particles available
                return;

            InitializeParticle(particle);
            pSystem.AddParticle(particle);
        }

        /// <summary>
        /// Initializes a particle before it is added to the particle system.
        /// Called from within GenerateParticle. Will always receive a non-null instance.
        /// </summary>
        /// <param name="particle"></param>
        protected abstract void InitializeParticle(T particle);
    }
}
