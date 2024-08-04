using MageTest.Gui.Interfaces;
using UnityEngine;
using Zenject;

namespace MageTest.Gui
{
    public abstract class BasePresenter<T> : SimplePresenter<T>
        where T : MonoBehaviour, IWindow
    {
        [Inject]
        private readonly GuiController _guiController;

        public sealed override void ShowWindow()
        {
            _guiController.RegisterPresenter(this);
            
            base.ShowWindow();
        }

        public sealed override void HideWindow()
        {
            base.HideWindow();
            
            _guiController.UnregisterPresenter(this);
        }
    }
}