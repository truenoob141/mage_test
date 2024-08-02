using UnityEngine;

namespace MageTest.Core.Interfaces
{
    public interface IEntity
    {
        public bool IsValid { get; }
        public Vector3 Position { get; }
        public float Size { get; }

        public Vector3 GetVelocity();
    }
}