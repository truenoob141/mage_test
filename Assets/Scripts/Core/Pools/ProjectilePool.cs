using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.Behaviours;
using MageTest.Core.Interfaces;
using MageTest.PoolSystem;
using UnityEngine.AddressableAssets;

namespace MageTest.Core.Pools
{
    public class ProjectilePool : DynamicPool<SimpleProjectileBehaviour>
    {
        public async UniTask<IProjectile> Spawn(
            AssetReferenceGameObject assetRef,
            ProjectileOptions options,
            CancellationToken token)
        {
            var instance = await Spawn(assetRef, token);
            instance.ApplyOptions(options);
            return instance;
        }
    }
}