using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.Behaviours;
using MageTest.Core.Configs;
using MageTest.Core.Interfaces;
using MageTest.PoolSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MageTest.Core.Pools
{
    public class EnemyPool : DynamicPool<EnemyBehaviour>
    {
        public async UniTask<IAliveEntity> Spawn(
            EnemyConfig config,
            Vector3 spawnPosition,
            CancellationToken token)
        {
            var instance = await Spawn(config._prefabRef, token);
            instance.transform.position = spawnPosition;
            instance.ApplyConfig(config);
            return instance;
        }
    }
}