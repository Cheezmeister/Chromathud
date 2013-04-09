using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WickedLibrary.Graphics.Hax;

namespace WickedLibrary.Graphics
{
    public class TextParticle : StdParticle
    {
        public SpriteFont Font;
        public string Text = string.Empty;
        public Vector2 Origin = Vector2.Zero;


        public override void Draw2D(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Font, Text, Position, Color, Rotation, Origin, Scale, SpriteEffects.None, Depth);
        }
    }


    public class TextParticleEmitter : StdEmitter<TextParticle>
    {
        public string Text {
            get { return text; }
            set {
                text = value ?? string.Empty; //Value if not null, string.Empty otherwise
                fontOrigin = font.MeasureString(text) / 2f;
            }
        }
        private string text = string.Empty;

        public SpriteFont Font { 
            get { return font; }
            set { font = value; fontOrigin = font.MeasureString(text) / 2f; }
        }
        private SpriteFont font;

        private Vector2 fontOrigin = Vector2.Zero;


        public TextParticleEmitter(ParticleSystem<TextParticle> system, Vector2 pos, SpriteFont font)
            : base(system, pos, null)
        {
            this.font = font;

            InitMinScale = InitMaxScale = 1f;
            EndMinScale = EndMaxScale = 1f;
            MinStartSpeed = MaxStartSpeed = 0;

            MinRotAngle = MaxRotAngle = 0;
            MinAngularSpeed = MaxAngularSpeed = 0;
            MinShootAngle = MaxShootAngle = 0;
            MinPlaceAngle = MaxPlaceAngle = 0;
            Radius = 0;
        }

        protected override void InitializeParticle(TextParticle p)
        {
            base.InitializeParticle(p);

            p.Font = font;
            p.Text = text;
            p.Origin = fontOrigin;
        }
    }






    public class TextTemplateEmitter : StdEmitter<StdParticle>
    {
        public TextTemplateGenerator Generator;
        public float Scale;
        public Vector2 Origin;


        public TextTemplateEmitter(ParticleSystem<StdParticle> system, TextureData particleTex, TextTemplateGenerator template, Vector2 pos, float scale, RegistrationLocation reg)
            : base(system, pos, particleTex)
        {
            this.Scale = scale;
            this.Generator = template;
            this.Origin = TextureData.GetOrigin(template.Width, template.Height, reg);

            InitMinScale = InitMaxScale = 1f;
            EndMinScale = EndMaxScale = 1f;
            MinStartSpeed = MaxStartSpeed = 0;
            MinAngularSpeed = MaxAngularSpeed = 0;
            MinShootAngle = 0;
            MaxShootAngle = MathHelper.TwoPi;
            MinPlaceAngle = 0;
            MaxPlaceAngle = MathHelper.TwoPi;
            Radius = 0;
        }

        protected override void InitializeParticle(StdParticle p)
        {
            base.InitializeParticle(p);

            p.Position = Generator.SelectRandom() * Scale + this.Position - this.Origin;
        }
    }
}