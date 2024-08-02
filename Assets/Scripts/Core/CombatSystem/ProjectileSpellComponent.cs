using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.Interfaces;
using MageTest.Core.Pools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace MageTest.Core.CombatSystem
{
    [CreateAssetMenu(fileName = "Projectile", menuName = "Spell Components/Projectile")]
    public class ProjectileSpellComponent : SpellComponent
    {
        [Inject]
        private readonly ProjectilePool _projectilePool;
        
        [SerializeField]
        private AssetReferenceGameObject _assetRef;
        [SerializeField]
        private float _speed = 1f;
        [SerializeField]
        private float _maxLifeTime = 5f;

        public override async UniTask<SpellComponentData> Create(SpellComponentData data, CancellationToken token)
        {
            var damage = data.Damage;
            var velocity = Helper.IsValid(data.Source)
                ? data.Source.GetVelocity()
                : Vector3.zero;
            
            var sourcePosition = Helper.IsValid(data.Source) 
                ? data.Source.Position 
                : data.SourcePosition.GetValueOrDefault();
            
            var tasks = new List<UniTask<IProjectile>>(data.TargetNumber);
            foreach (var target in data.GetTargets())
            {
                var options = new ProjectileOptions(
                    sourcePosition,
                    target.Position,
                    velocity,
                    _speed,
                    damage);
                
                tasks.Add(_projectilePool.Spawn(_assetRef, options, token));
            }

            if (data.TargetPosition.HasValue)
            {
                var options = new ProjectileOptions(
                    sourcePosition,
                    data.TargetPosition.Value,
                    velocity,
                    _speed,
                    damage);
                
                tasks.Add(_projectilePool.Spawn(_assetRef, options, token));
            }
            
            var projectiles = await UniTask.WhenAll(tasks);

            var cts = new CancellationTokenSource();
            var despawnToken = CancellationTokenSource.CreateLinkedTokenSource(
                token,
                cts.Token).Token;
            foreach (var projectile in projectiles)
            {
                var current = projectile;
                projectile.Fire(despawnToken)
                    .ContinueWith(() =>
                    {
                        _projectilePool.Despawn(current);
                        cts.Cancel();
                    })
                    .Forget();
            }

            if (_maxLifeTime > 0)
            {
                UniTask.Delay(TimeSpan.FromSeconds(_maxLifeTime), cancellationToken: despawnToken)
                    .ContinueWith(() =>
                    {
                        foreach (var projectile in projectiles.Where(Helper.IsValid))
                            _projectilePool.Despawn(projectile);

                        cts.Cancel();
                    })
                    .Forget();
            }

            return data;
        }
    }
}