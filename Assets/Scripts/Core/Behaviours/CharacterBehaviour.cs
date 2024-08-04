using System;
using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.CombatSystem;
using MageTest.Core.Configs;
using MageTest.Core.Controllers;
using MageTest.Core.Factories;
using MageTest.Core.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace MageTest.Core.Behaviours
{
    [RequireComponent(typeof(CircleCollider2D)), RequireComponent(typeof(Rigidbody2D))]
    public class CharacterBehaviour : MonoBehaviour, IAliveEntity
    {
        [Inject]
        private readonly SpellFactory _spellFactory;
        [Inject]
        private readonly PlayerController _playerController;

        [SerializeField]
        private InputActionReference _moveInputAction;
        [SerializeField]
        private InputActionReference _rotateInputAction;
        [SerializeField]
        private InputActionReference _attackInputAction;
        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        [SerializeField]
        private Sprite[] _sprites;

        public bool IsValid => _health > 0;
        public Vector3 Position => transform.position;
        public float Size => _collider == null ? 1f : _collider.radius * 2;

        public event Action<IAliveEntity> OnDead;

        private Camera _cam;
        private Rigidbody2D _rigidbody;
        private CircleCollider2D _collider;
        private CancellationToken _token;

        private float _lastCastSpellTime = float.MinValue;
        private float _moveSpeed;
        private float _health;
        private float _defenseMultiplier;
        private float _spellDelay;

        private int _rotateIndex;

        private void Start()
        {
            _cam = Camera.main;
            _token = this.GetCancellationTokenOnDestroy();
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();
        }

        private void OnEnable()
        {
            _moveInputAction.action.Enable();
            _rotateInputAction.action.Enable();
            _attackInputAction.action.Enable();
        }

        private void OnDisable()
        {
            _moveInputAction.action.Disable();
            _rotateInputAction.action.Disable();
            _attackInputAction.action.Disable();
        }

        private void LateUpdate()
        {
            if (!IsValid)
                return;
            
            var dir = _rotateInputAction.action.ReadValue<Vector2>();
            Rotate(dir);

            if (_attackInputAction.action.IsPressed())
                CastSpell();

            float halfSize = Size * 0.5f;
            float height = _cam.orthographicSize;
            float width = height * _cam.aspect;
            
            var pos = transform.position;
            if (pos.x > width - halfSize)
                pos.x = width - halfSize;
            else if (pos.x < -width + halfSize)
                pos.x = -width + halfSize;

            if (pos.y > height - halfSize)
                pos.y = height - halfSize;
            else if (pos.y < -height + halfSize)
                pos.y = -height + halfSize;

            transform.position = pos;
        }

        private void FixedUpdate()
        {
            if (!IsValid)
                return;
            
            var value = _moveInputAction.action.ReadValue<Vector2>();
            _rigidbody.velocity = value * _moveSpeed;
        }

        public void Respawn(PlayerConfig config)
        {
            Rotate(Vector2.up);
            
            _moveSpeed = config._moveSpeed;
            _health = config._health;
            _defenseMultiplier = 1 - Mathf.Clamp01(config._defense * 0.01f);
            _spellDelay = config._spellDelay;

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }

        public Vector3 GetVelocity()
        {
            return _rigidbody.velocity;
        }

        public void TakeDamage(int damage)
        {
            if (_health <= 0)
                return;

            _health -= damage * _defenseMultiplier;
            if (_health <= 0)
                OnDead?.Invoke(this);
        }

        private void Rotate(Vector2 dir)
        {
            if (dir == Vector2.zero)
                return;

            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            int index = (int) (angle / 45);
            if (index < 0)
                index = 8 + index;

            _rotateIndex = index;
            _spriteRenderer.sprite = _sprites[index];
            // transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
        }

        private void CastSpell()
        {
            if (_lastCastSpellTime + _spellDelay > Time.time)
                return;

            _lastCastSpellTime = Time.time;

            // var lookDir = transform.rotation * Vector3.up;
            // With velocity
            // Vector3 moveDir = _rigidbody.velocity.normalized;
            // float dot = Vector3.Dot(moveDir, lookDir);
            // var targetPos = transform.position + 99999 * (dot > 0.9f || dot < -0.9f ? lookDir : moveDir + lookDir);
            var lookDir = Quaternion.Euler(0, 0, (_rotateIndex * -45)) * Vector3.up;
            var targetPos = transform.position + lookDir * 99999;

            var currentSpell = _playerController.GetCurrentSpell();
            var spell = _spellFactory.Create(currentSpell);
            spell.Cast(this, targetPos, _token).Forget();
        }
    }
}