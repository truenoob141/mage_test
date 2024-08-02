using System;

namespace MageTest.Core.Interfaces
{
    public interface IAliveEntity : IEntity
    {
        public event Action<IAliveEntity> OnDead;

        public void TakeDamage(int damage);
    }
}