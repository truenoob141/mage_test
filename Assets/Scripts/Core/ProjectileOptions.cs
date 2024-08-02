using UnityEngine;

namespace MageTest.Core
{
    public sealed class ProjectileOptions
    {
        public Vector3 BeginPosition { get; }
        public Vector3 EndPosition { get; }
        public Vector3 InitialVelocity { get; }
        public float Speed { get; }
        public int Damage { get; }

        public ProjectileOptions(
            Vector3 beginPosition, 
            Vector3 endPosition,
            Vector3 initialVelocity,
            float speed,
            int damage)
        {
            BeginPosition = beginPosition;
            EndPosition = endPosition;
            InitialVelocity = initialVelocity;
            Speed = speed;
            Damage = damage;
        }
    }
}