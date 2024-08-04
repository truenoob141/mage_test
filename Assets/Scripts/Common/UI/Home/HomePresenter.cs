using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core;
using MageTest.Gui;
using UnityEngine;
using Zenject;

namespace MageTest.Common.UI.Home
{
    public class HomePresenter : BasePresenter<HomeView>
    {
        [Inject]
        private readonly EventDispatcher _eventDispatcher;
        [Inject]
        private readonly GuiController _guiController;
        
        public override UILayerEnum Layer => UILayerEnum.Main;
        public override UIFlagsEnum Flags => UIFlagsEnum.DisableAutoStash;

        public new UniTask<bool> LoadAndShowWindow(CancellationToken token)
        {
            return base.LoadAndShowWindow(token);
        }

        protected override void OnShow()
        {
            view.OnStart += OnStartHandler;
            
            _eventDispatcher.Subscribe<OnGameStart>(OnGameStartHandler);
            _eventDispatcher.Subscribe<OnGameStarted>(OnGameStartedHandler);
        }

        protected override void OnHide()
        {
            view.OnStart -= OnStartHandler;

            _eventDispatcher.Unsubscribe<OnGameStart>(OnGameStartHandler);
            _eventDispatcher.Unsubscribe<OnGameStarted>(OnGameStartedHandler);
        }

        private void OnGameStartHandler()
        {
            _guiController.SetThrobber(this);
        }

        private void OnGameStartedHandler()
        {
            _guiController.RemoveThrobber(this);
            HideWindow();
        }

        private void OnStartHandler()
        {
            _eventDispatcher.Trigger<OnGameStart>();
        }
    }
}