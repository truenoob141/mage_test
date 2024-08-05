using System;

namespace MageTest.Core.Interfaces
{
    public interface IAliveEntity : IEntity
    {
        public float Health { get; }
        public int MaxHealth { get; }

        public event Action<IAliveEntity> OnDead;

        public void TakeDamage(int damage);
        public void Heal(int amount);
    }
}