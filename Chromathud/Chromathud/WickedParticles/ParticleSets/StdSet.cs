using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WickedLibrary.Graphics.ParticleSets;
using WickedLibrary.Graphics.Hax;

namespace WickedLibrary.Graphics
{
    /// <summary>
    /// A standard physics-based particle that scales and tints over time
    /// </summary>
    public class StdParticle : PhysicsParticle
    {
        public float InitScale;
        public float EndScale;
        public float Scale;

        public TextureData Texture;

        public Vector4 InitColor 
        {
            get { return initColor.ToVector4(); }
            set { initColor = new Color(value); } 
        }
        private Color initColor;

        public Vector4 EndColor
        {
            get { return endColor.ToVector4(); }
            set { endColor = new Color(value); } 
        }
        private Microsoft.Xna.Framework.Color endColor;

        protected Color Color;

        //Draw depth
        public float Depth;


        public override void Update(float elapsedTime)
        {
            base.Update(elapsedTime);

            float percent = Age / MaxAge;

            Color.R = Interpolator.LinearInterp(initColor.R, endColor.R, percent);
            Color.G = Interpolator.LinearInterp(initColor.G, endColor.G, percent);
            Color.B = Interpolator.LinearInterp(initColor.B, endColor.B, percent);
            Color.A = Interpolator.LinearInterp(initColor.A, endColor.A, percent);

            Scale = Interpolator.LinearInterp(InitScale, EndScale, percent);
        }

        public override void Draw2D(SpriteBatch spriteBatch)
        {
            if (Texture == null)
                return;

            spriteBatch.Draw(Texture.Texture, Position, null, Color, Rotation, Texture.Origin, Scale, SpriteEffects.None, Depth);
        }
    }

    public class StdEmitter<T> : EmitterBase<T> where T : StdParticle, new()
    {
        #region Rate Settings
        /// <summary>
        /// The amount of particles per second to be added to the system
        /// </summary>
        public float BirthRate
        {
            get { return birthRate; }
            set
            {
                birthRate = Math.Max(0, value);
                releaseRate = 1.0f / birthRate;
            }
        }
        float birthRate = 4;

        /// <summary>
        /// Amount of time between particle creation. Set by BirthRate
        /// </summary>
        float releaseRate = 0.25f;

        /// <summary>
        /// Timer for calculating the release
        /// </summary>
        float releaseTimer = 0;
        #endregion


        /// <summary>
        /// The location of the emitter's center
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        protected Vector2 position;
        protected Vector2 prevPosition;

        /// <summary>
        /// The area around the emitter to spawn particles
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set { radius = Math.Max(0, value); }
        }
        protected float radius = 0;

        /// <summary>
        /// The texture to use when drawing each particle
        /// </summary>
        public TextureData Texture;

        /// <summary>
        /// The tint of the particle at creation
        /// </summary>
        public Vector4 InitColor = Color.White.ToVector4();

        /// <summary>
        /// The tint of the particle at death
        /// </summary>
        public Vector4 EndColor = Color.White.ToVector4();

        //-------------------------------------------
        public float InitMinScale = 1.0f;
        public float InitMaxScale = 1.0f;

        public float EndMinScale = 1.0f;
        public float EndMaxScale = 1.0f;
        //-------------------------------------------

        //-------------------------------------------
        //Initial angle
        public float MinRotAngle = -MathHelper.Pi;
        public float MaxRotAngle = MathHelper.Pi;

        //Initial angular speed
        public float MinAngularSpeed = 0;
        public float MaxAngularSpeed = 0;
        //-------------------------------------------

        public float MinPlaceAngle = -MathHelper.Pi;
        public float MaxPlaceAngle = MathHelper.Pi;

        public float MinShootAngle = -MathHelper.Pi;
        public float MaxShootAngle = MathHelper.Pi;

        public float MinStartSpeed = 1f;
        public float MaxStartSpeed = 10f;

        //-------------------------------------------
        public float MinParticleLongevity = 2;
        public float MaxParticleLongevity = 5;

        private Vector2 particlePos = Vector2.Zero;


        public StdEmitter(ParticleSystem<T> particleSystem, Vector2 position, TextureData textureData)
            : base(particleSystem)
        {
            this.Position = this.prevPosition = position;
            this.Texture = textureData;
        }



        public override void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime > 0)
            {
                Vector2 velocity = (Position - prevPosition) / elapsedTime;

                //If we suddenly moved really far, we probably shouldn't be interpolating over the distance
                bool jumped = velocity.Length() > 400;

                float timeToSpend = releaseTimer + elapsedTime;
                float currentTime = -releaseTimer;

                while (timeToSpend >= releaseRate)
                {
                    currentTime += releaseRate;
                    timeToSpend -= releaseRate;

                    // Work out the optimal position for this particle. This will produce
                    // evenly spaced particles regardless of the object speed, particle
                    // creation frequency, or game update rate.
                    float mu = currentTime / elapsedTime;

                    particlePos = (jumped) ? Position : Vector2.Lerp(prevPosition, Position, mu);

                    // Create the particle.
                    GenerateParticle();
                }
                releaseTimer = timeToSpend;
            }
            prevPosition = Position;
        }

        protected override void InitializeParticle(T p)
        {
            Random random = MathUtility.Random;

            float rotAngle = MathUtility.RandomFloat(MinRotAngle, MaxRotAngle);
            float placeAngle = MathUtility.RandomFloat(MinPlaceAngle, MaxPlaceAngle);
            float shootAngle = MathUtility.RandomFloat(MinShootAngle, MaxShootAngle);
            float shootSpeed = MathUtility.RandomFloat(MinStartSpeed, MaxStartSpeed);
            float startScale = MathUtility.RandomFloat(InitMinScale, InitMaxScale);
            float endScale = MathUtility.RandomFloat(EndMinScale, EndMaxScale);
            float angSpeed = MathUtility.RandomFloat(MinAngularSpeed, MaxAngularSpeed);
            float maxAge = MathUtility.RandomFloat(MinParticleLongevity, MaxParticleLongevity);

            p.Position = particlePos + MathUtility.ToCartesian(placeAngle, (float)(Radius * random.NextDouble()));
            p.Velocity = MathUtility.ToCartesian(shootAngle, shootSpeed);
            p.Rotation = rotAngle;
            p.AngularVelocity = angSpeed;
            p.Texture = Texture;
            p.MaxAge = maxAge;
            p.Depth = 0;

            p.InitColor = InitColor;
            p.EndColor = EndColor;

            p.InitScale = startScale;
            p.EndScale = endScale;
        }
    }
}
