using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WickedLibrary.Graphics.Hax;

namespace WickedLibrary.Graphics.ParticleSets
{
    /// <summary>
    /// Represents a basic particle with physical properties like mass and velocity
    /// </summary>
    public class PhysicsParticle : Particle
    {
        /// <summary>
        /// Position of the particle on screen
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Mass of particle. Changes how applied forces affect it.
        /// </summary>
        public float Mass
        {
            get { return mass; }
            set
            {
                mass = Math.Max(value, Hax.MathUtility.Epsilon);
                invMass = 1.0f / mass;
            } //Mass cannot be 0!
        }
        private float mass = 1.0f;
        private float invMass = 1.0f;

        /// <summary>
        /// Moment of inertia. Like mass for rotation
        /// </summary>
        public float MoI
        {
            get { return moi; }
            set
            {
                moi = Math.Max(value, Hax.MathUtility.Epsilon);
                invMoi = 1 / moi;
            } //Mass cannot be 0!
        }
        private float moi = 1.0f;
        private float invMoi = 1.0f;

        /// <summary>
        /// 2D Rotation of the particle
        /// </summary>
        public float Rotation;

        private Vector2 totalForce;
        public Vector2 Velocity;

        private float totalTorque;
        public float AngularVelocity;



        /// <summary>
        /// Subjects the particle to a force for this frame
        /// </summary>
        /// <param name="force"></param>
        public void ApplyForce(Vector2 force)
        {
            totalForce += force;
        }

        /// <summary>
        /// Subjects the particle to a rotational force (torque)
        /// </summary>
        /// <param name="force"></param>
        public void ApplyTorque(float force)
        {
            totalTorque += force;
        }

        /// <summary>
        /// Applies a force to a given point on the particle, which
        /// will be resolved into a force on the center of mass and a torque
        /// </summary>
        /// <param name="force">The force to apply</param>
        /// <param name="point">The point of application (in world space)</param>
        public void ApplyForce(Vector2 force, Vector2 point)
        {
            if (force == Vector2.Zero)
                return;

            Vector2 toPoint = point - Position;
            if (toPoint == Vector2.Zero)
            {
                totalForce += force;
                return;
            }

            toPoint.Normalize();
            Vector2 right = new Vector2(toPoint.Y, -toPoint.X);

            float forceParallel, forcePerp;

            Vector2.Dot(ref force, ref toPoint, out forceParallel);
            Vector2.Dot(ref force, ref right, out forcePerp);

            totalForce += forceParallel * toPoint;
            totalTorque += forcePerp;
        }


        public override void Update(float elapsedTime)
        {
            base.Update(elapsedTime);

            float massTime = (elapsedTime * invMass);
            Velocity.X += totalForce.X * massTime;
            Velocity.Y += totalForce.Y * massTime;

            float moiTime = (elapsedTime * invMoi);
            AngularVelocity += totalTorque * moiTime;

            Position += Velocity * elapsedTime;
            Rotation += AngularVelocity * elapsedTime;

            totalForce.X = 0;
            totalForce.Y = 0;
            totalTorque = 0;
        }

        public override void Reset()
        {
            base.Reset();

            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            totalForce = Vector2.Zero;

            Rotation = 0;
            AngularVelocity = 0;
            totalTorque = 0;

            moi = invMoi = 1;
            mass = invMass = 1;
        }
    }
}
