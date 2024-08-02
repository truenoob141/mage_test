using System;
using System.Linq;
using MageTest.Core.Behaviours;
using MageTest.Core.CombatSystem;
using MageTest.Core.Interfaces;
using UnityEngine.Assertions;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MageTest.Core.Controllers
{
    public class PlayerController : IInitializable
    {
        [Inject]
        private readonly GameSettings _gameSettings;

        public event Action OnPlayerRespawned;
        
        private CharacterBehaviour _player;

        private int _selectedSpell;
        private Spell[] _spells;

        public Spell GetCurrentSpell()
        {
            return _spells[_selectedSpell];
        }
        
        public Spell GetCurrentSpell(out int index)
        {
            if (_spells == null)
            {
                index = -1;
                return null;
            }

            index = _selectedSpell;
            return _spells[_selectedSpell];
        }

        public Spell[] GetAllSpells()
        {
            return _spells ?? Array.Empty<Spell>();
        }

        public bool ChooseNextSpell(bool reverse, out int index)
        {
            index = _selectedSpell;
            int i = index;
            if (reverse)
            {
                if (--i < 0)
                    return false;
            }
            else if (++i >= _spells.Length)
                return false;

            _selectedSpell = index = i;
            return true;
        }

        public IAliveEntity GetPlayer()
        {
            if (_player == null)
            {
                var player = Object.FindFirstObjectByType<CharacterBehaviour>();
                player.OnDead += OnPlayerDead;
                
                _player = player;
                Respawn();
            }

            return _player;
        }

        private void OnPlayerDead(IAliveEntity entity)
        {
            Assert.AreEqual(entity, _player);
            Respawn();
        }

        private void Respawn()
        {
            var availableSpells = _gameSettings._availableSpells;
            _selectedSpell = 0;
            _spells = availableSpells.OrderBy(_ => Random.value)
                .Take(_gameSettings._maxSpells)
                .ToArray();

            var config = _gameSettings._playerConfig;
            _player.Respawn(config);

            OnPlayerRespawned?.Invoke();
        }

        public void Initialize()
        {
            // Initialize player!
            GetPlayer();
        }
    }
}