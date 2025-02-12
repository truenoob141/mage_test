﻿using System.Linq;
using MageTest.Core.Controllers;
using MageTest.Core.Interfaces;
using MageTest.Gui;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace MageTest.Core.UI.Combat
{
    public class CombatView : BaseView
    {
        [Inject]
        private readonly PlayerController _playerController;

        [SerializeField]
        private InputActionReference _changeSpellAction;
        [SerializeField]
        private Slider _healthBar;
        [SerializeField]
        private Image[] _icons;
        [SerializeField]
        private Toggle[] _toggles;

        private IAliveEntity _player;
        
        protected override void OnEnable()
        {
            _changeSpellAction.action.Enable();
            _changeSpellAction.action.performed += OnChangeSpellAction;

            _playerController.OnPlayerRespawned += OnPlayerRespawned;
            OnPlayerRespawned();
        }

        protected override void OnDisable()
        {
            _changeSpellAction.action.performed -= OnChangeSpellAction;
            _changeSpellAction.action.Disable();

            _playerController.OnPlayerRespawned -= OnPlayerRespawned;
        }

        private void Update()
        {
            if (!Helper.IsValid(_player))
            {
                _healthBar.value = 1;
                return;
            }
            
            _healthBar.value = Mathf.Clamp01(_player.Health / _player.MaxHealth);
        }

        private void OnChangeSpellAction(InputAction.CallbackContext context)
        {
            float dir = context.ReadValue<float>();

            bool success = _playerController.ChooseNextSpell(dir < 0, out int index);
            if (success && index != -1)
                _toggles[index].isOn = true;
        }

        private void OnPlayerRespawned()
        {
            Assert.AreEqual(_toggles.Length, _icons.Length);

            _player = _playerController.GetPlayer();
            
            int index;
            var spell = _playerController.GetCurrentSpell(out index);
            if (spell == null)
            {
                foreach (var toggle in _toggles.Where(t => t.isOn))
                    toggle.isOn = false;

                return;
            }

            var spells = _playerController.GetAllSpells();
            for (int i = 0; i < _icons.Length; i++)
            {
                var icon = _icons[i];
                if (i >= spells.Length)
                {
                    icon.gameObject.SetActive(false);
                    continue;
                }

                icon.sprite = spells[i].GetIcon();
                icon.gameObject.SetActive(true);
            }

            for (int i = spells.Length; i < _icons.Length; ++i)
            {
                _icons[i].gameObject.SetActive(false);
            }

            _toggles[index].isOn = true;
        }
    }
}