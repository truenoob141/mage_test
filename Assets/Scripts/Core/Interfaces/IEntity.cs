using UnityEngine;

namespace MageTest.Core.Interfaces
{
    /// <summary>
    /// Entity is an object that can be interacted with in the game world.
    /// Like a player, enemy, projectile, etc.
    /// </summary>
    public interface IEntity
    {
        public bool IsValid { get; }
        public Vector3 Position { get; }
        public float Size { get; }

        public Vector3 GetVelocity();
    }
}