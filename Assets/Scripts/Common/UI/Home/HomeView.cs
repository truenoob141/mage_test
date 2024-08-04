using System;
using MageTest.Gui;
using UnityEngine.InputSystem;

namespace MageTest.Common.UI.Home
{
    public class HomeView : BaseView
    {
        public event Action OnStart;
        
        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.anyKey.wasPressedThisFrame)
                OnStart?.Invoke();
        }
    }
}