using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.CombatSystem;
using MageTest.Core.Configs;
using MageTest.Core.Controllers;
using MageTest.Core.Factories;
using MageTest.Core.Interfaces;
using UnityEngine;
using Zenject;

namespace MageTest.Core.Behaviours
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class EnemyBehaviour : MonoBehaviour, IAliveEntity
    {
        [Inject]
        private readonly PlayerController _playerController;
        [Inject]
        private readonly SpellFactory _spellFactory;

        public bool IsValid => _health > 0;
        public float Health => _health;
        public int MaxHealth => _maxHealth;
        public Vector3 Position => transform.position;
        public float Size => _collider == null ? 1f : _collider.radius * 2;

        public event Action<IAliveEntity> OnDead;

        private float _health;
        private int _maxHealth;
        private float _defenseMultiplier;
        private float _moveSpeed;
        private float _spellDelay;
        private Spell _spell;

        private float _lastCastSpellTime = float.MinValue;
        private CancellationToken _token;

        private CircleCollider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
        }

        private void Start()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        private void Update()
        {
            if (!IsValid)
                return;
            
            var target = _playerController.GetPlayer();
            if (!Helper.IsValid(target))
                return;

            var currentPos = Position;
            var targetPos = target.Position;
            var dir = (targetPos - currentPos).normalized;
            targetPos -= dir * target.Size;

            transform.rotation = Helper.LookAt2D(dir);
            var pos = Vector3.MoveTowards(currentPos, targetPos, _moveSpeed * Time.deltaTime);
            transform.position = pos;
            if (pos == targetPos)
                CastSpell(target);
        }
        
        public void ApplyConfig(EnemyConfig config)
        {
            _health = config._health;
            _maxHealth = config._health;
            _defenseMultiplier = 1 - Mathf.Clamp01(config._defense * 0.01f);
            _moveSpeed = config._moveSpeed;
            _spellDelay = config._spellDelay;
            _spell = config._spell;
        }

        public void TakeDamage(int value)
        {
            if (_health <= 0)
                return;
            
            _health -= value * _defenseMultiplier;
            if (_health <= 0)
                OnDead?.Invoke(this);
        }
        
        public void Heal(int amount)
        {
            if (!IsValid)
                return;

            _health = Mathf.Min(_maxHealth, _health + amount);
        }

        public Vector3 GetVelocity()
        {
            return Vector3.zero;
        }

        private void CastSpell(IEntity entity)
        {
            if (_lastCastSpellTime + _spellDelay > Time.time)
                return;

            _lastCastSpellTime = Time.time;
            
            var spell = _spellFactory.Create(_spell);
            spell.Cast(this, entity, _token).Forget();
        }
    }
}