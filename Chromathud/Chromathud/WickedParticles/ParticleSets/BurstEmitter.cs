using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WickedLibrary.Graphics.Hax;
#if USING_BURST_EMITTER_STUFF
namespace WickedLibrary.Graphics.ParticleSets
{
    public class BurstEmitter : EmitterBase<StdParticle>
    {
        public Vector2 Position;

        public int MinBurstParticles = 10;
        public int MaxBurstParticles = 10;

        public float MinBurstTime
        {
            get { return _minBurstTime; }
            set { _minBurstTime = value;
                burstTime = MathUtility.RandomFloat(_minBurstTime, _maxBurstTime);
            }
        }
        private float _minBurstTime = 2;

        public float MaxBurstTime
        {
            get { return _maxBurstTime; }
            set
            {
                _maxBurstTime = value;
                burstTime = MathUtility.RandomFloat(_minBurstTime, _maxBurstTime);
            }
        }
        private float _maxBurstTime = 2;


        private float burstTime = 2;


        //-------------------------------------------
        public float InitMinScale = 1.0f;
        public float InitMaxScale = 1.0f;

        public float EndMinScale = 1.0f;
        public float EndMaxScale = 1.0f;
        //-------------------------------------------

        //-------------------------------------------
        //Initial angular speed
        public float MinAngularSpeed = 0;
        public float MaxAngularSpeed = 0;
        //-------------------------------------------

        public float MinStartSpeed = 50f;
        public float MaxStartSpeed = 50f;

        public float MinParticleLongevity = 3;
        public float MaxParticleLongevity = 3;


        /// <summary>
        /// The tint of the particle at creation
        /// </summary>
        public Vector4 InitColor = Color.White.ToVector4();

        /// <summary>
        /// The tint of the particle at death
        /// </summary>
        public Vector4 EndColor = Color.White.ToVector4();

        /// <summary>
        /// The texture to use when drawing each particle
        /// </summary>
        public TextureData Texture;



        private float elapsedTime = 0;
        private int numCreated = 0;
        private int particlesToCreate = 0;


        public BurstEmitter(ParticleSystem<StdParticle> system, Vector2 position, TextureData tex) : base(system)
        {
            this.Position = position;
            this.Texture = tex;
        }


        public override void Update(GameTime gameTime)
        {
            float eTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            elapsedTime += eTime;

            if(elapsedTime > burstTime)
            {
                elapsedTime -= burstTime;

                CreateBurst();
                burstTime = MathUtility.RandomFloat(_minBurstTime, _maxBurstTime);
            }
        }


        public void CreateBurst()
        {
            particlesToCreate = MathUtility.Random.Next(MinBurstParticles, MaxBurstParticles);

            for (numCreated = 0; numCreated < particlesToCreate; numCreated++)
            {
                GenerateParticle();
            }
        }


        protected override void InitializeParticle(StdParticle p)
        {
            float shootAngle = (MathHelper.TwoPi / particlesToCreate) * numCreated;
            float shootSpeed = MathUtility.RandomFloat(MinStartSpeed, MaxStartSpeed);
            float startScale = MathUtility.RandomFloat(InitMinScale, InitMaxScale);
            float endScale = MathUtility.RandomFloat(EndMinScale, EndMaxScale);
            float angSpeed = MathUtility.RandomFloat(MinAngularSpeed, MaxAngularSpeed);
            float maxAge = MathUtility.RandomFloat(MinParticleLongevity, MaxParticleLongevity);

            p.Position = Position;
            p.Velocity = MathUtility.ToCartesian(shootAngle, shootSpeed);
            p.Rotation = shootAngle;
            p.AngularVelocity = angSpeed;
            p.Texture = Texture;
            p.MaxAge = maxAge;

            p.InitScale = startScale;
            p.EndScale = endScale;

            p.InitColor = InitColor;
            p.EndColor = EndColor;
        }
    }
}
#endif