using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Common.UI.Home;
using UnityEngine;
using Zenject;

namespace MageTest
{
    public class Bootstrap : MonoBehaviour
    {
        [Inject]
        private readonly HomePresenter _homePresenter;
        
        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            Init(token).Forget();
        }

        private async UniTaskVoid Init(CancellationToken token)
        {
            await _homePresenter.LoadAndShowWindow(token);
        }
    }
}