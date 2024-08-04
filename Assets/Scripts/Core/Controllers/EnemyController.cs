using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.Interfaces;
using MageTest.Core.Pools;
using MageTest.Core.Services;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;
using Random = UnityEngine.Random;

namespace MageTest.Core.Controllers
{
    public class EnemyController : IInitializable, IDisposable, ITickable
    {
        [Inject]
        private readonly EnemyPool _enemyPool;
        [Inject]
        private readonly EventDispatcher _eventDispatcher;
        [Inject]
        private readonly GameService _gameService;
        [Inject]
        private readonly GameSettings _gameSettings;
        [Inject]
        private readonly PlayerController _playerController;

        private float _nextSpawn;
        private int _enemyCount;

        private Camera _cam;
        private readonly List<IAliveEntity> _enemies = new();

        private IAliveEntity _player;
        private CancellationTokenSource _cts;

        public void Initialize()
        {
            _cts = new CancellationTokenSource();

            _eventDispatcher.Subscribe<OnGameStarted>(OnGameStartedHandler);
            _eventDispatcher.Subscribe<OnGameEnd>(OnGameEndHandler);
        }

        public void Dispose()
        {
            _eventDispatcher.Unsubscribe<OnGameStarted>(OnGameStartedHandler);
            _eventDispatcher.Unsubscribe<OnGameEnd>(OnGameEndHandler);
            
            _cts?.Cancel();
        }

        private void OnGameStartedHandler()
        {
            _player = _playerController.GetPlayer();
            _player.OnDead += OnPlayerDeadHandler;
        }

        private void OnGameEndHandler()
        {
            _player.OnDead -= OnPlayerDeadHandler;
        }

        public void Tick()
        {
            if (!_gameService.IsValidGame)
                return;

            if (_enemyCount >= _gameSettings._maxEnemies || Time.time < _nextSpawn)
                return;

            var delay = _gameSettings._enemySpawnDelayRange;
            _nextSpawn = Time.time + Random.Range(delay.x, delay.y);

            var config = _gameSettings._enemyConfigs.OrderBy(_ => Random.value).FirstOrDefault();
            if (config == null)
                return;

            ++_enemyCount;

            if (_cam == null)
            {
                _cam = Camera.main;
                Assert.IsNotNull(_cam);
            }

            Vector3 randomDir = Random.insideUnitCircle.normalized;
            float width = _cam.orthographicSize * _cam.aspect;
            var token = _cts.Token;
            _enemyPool.Spawn(config,
                    _cam.transform.position +
                    new Vector3(randomDir.x * (width + 3),
                        randomDir.y * (_cam.orthographicSize + 3),
                        0),
                    token)
                .ContinueWith(e =>
                {
                    e.OnDead += OnEntityDead;
                    _enemies.Add(e);
                });
        }

        private void OnEntityDead(IAliveEntity entity)
        {
            --_enemyCount;
            entity.OnDead -= OnEntityDead;
            _enemyPool.Despawn(entity);
            _enemies.Remove(entity);
        }

        private void OnPlayerDeadHandler(IAliveEntity entity)
        {
            _enemyCount = 0;
            _enemies.ForEach(e => e.OnDead -= OnEntityDead);
            _enemyPool.Despawn(_enemies);
            _enemies.Clear();
        }
    }
}