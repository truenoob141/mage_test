using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.Services;
using MageTest.Core.UI.Combat;
using UnityEngine;
using Zenject;

namespace MageTest.Core.Controllers
{
    public class CombatController : IInitializable, IDisposable
    {
        [Inject]
        private readonly CombatPresenter _combatPresenter;
        [Inject]
        private readonly EventDispatcher _eventDispatcher;
        [Inject]
        private readonly GameService _gameService;
        [Inject]
        private readonly PlayerController _playerController;

        private CancellationTokenSource _cts;

        public void Initialize()
        {
            _cts = new CancellationTokenSource();

            _eventDispatcher.Subscribe<OnGameStart>(OnGameStartHandler);
        }

        public void Dispose()
        {
            _eventDispatcher.Unsubscribe<OnGameStart>(OnGameStartHandler);

            _cts.Cancel();
        }

        private void OnGameStartHandler()
        {
            var token = _cts.Token;
            StartGame(token).Forget();
        }

        private async UniTaskVoid StartGame(CancellationToken token)
        {
            bool success = await _combatPresenter.LoadAndShowWindow(token);
            if (!success)
            {
                Debug.LogError("Failed to start game: Failed to load combat view");
                return;
            }

            _gameService.SetIsValidGame(true);

            _eventDispatcher.Trigger<OnGameStarted>();
        }
    }
}