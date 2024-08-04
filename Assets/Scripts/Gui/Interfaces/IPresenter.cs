using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MageTest.Gui.Interfaces
{
    public interface IPresenter
    {
        public bool IsShown { get; }
        public bool IsLoading { get; }
        public bool IsClosing { get; }
        UIFlagsEnum Flags { get; }
        UILayerEnum Layer { get; }

        UniTask<bool> LoadWindow(CancellationToken token);
        void CloseWindow(bool force = false);
        
        void ShowWindow();
        void HideWindow();

        Transform GetViewTransform();
    }
}