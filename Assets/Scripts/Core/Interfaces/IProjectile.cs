using System.Threading;
using Cysharp.Threading.Tasks;

namespace MageTest.Core.Interfaces
{
    public interface IProjectile : IEntity
    {
        public UniTask Fire(CancellationToken token);
    }
}