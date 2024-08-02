using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.Interfaces;
using UnityEngine;

namespace MageTest.Core.Behaviours
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class SimpleProjectileBehaviour : MonoBehaviour, IProjectile
    {
        public bool IsValid => isActiveAndEnabled;
        public Vector3 Position => transform.position;
        public float Size => _collider == null ? 1 : _collider.radius * 2;

        private UniTaskCompletionSource _utcs;

        private float _speed;
        private int _damage;
        private float _beginTime;
        private float _endTime;

        private Vector3 _beginPosition;
        private Vector3 _endPosition;

        private CircleCollider2D _collider;
        
        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
        }

        private void OnDisable()
        {
            if (_utcs != null)
            {
                var utcs = _utcs;
                _utcs = null;
                utcs.TrySetCanceled();
            }
        }

        private void OnDestroy()
        {
            if (_utcs != null)
            {
                var utcs = _utcs;
                _utcs = null;
                utcs.TrySetCanceled();
            }
        }

        private void Update()
        {
            float progress = Mathf.Clamp01((Time.time - _beginTime) / _endTime);
            transform.position = Vector3.Lerp(_beginPosition, _endPosition, progress);

            if (progress >= 1 && _utcs != null)
            {
                var utcs = _utcs;
                _utcs = null;
                utcs.TrySetResult();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsValid)
                return;

            if (other.CompareTag(Tags.aliveEntities))
            {
                var entity = other.GetComponent<IAliveEntity>();
                if (!Helper.IsValid(entity))
                    return;
                
                entity.TakeDamage(_damage);
                
                if (_utcs != null)
                {
                    var utcs = _utcs;
                    _utcs = null;
                    utcs.TrySetResult();
                }
            }
        }

        public UniTask Fire(CancellationToken token)
        {
            _utcs?.TrySetCanceled();
            _utcs = new UniTaskCompletionSource();

            _beginTime = Time.time;
            _endTime = (_endPosition - _beginPosition).magnitude / _speed;

            return _utcs.Task;
        }

        public void ApplyOptions(ProjectileOptions options)
        {
            _beginPosition = options.BeginPosition;
            _endPosition = options.EndPosition;
            _speed = options.Speed;
            _damage = options.Damage;
            
            transform.position = _beginPosition;
            Helper.LookAt2D(transform, _endPosition - _beginPosition);
        }
        
        public Vector3 GetVelocity()
        {
            return (_endPosition - _beginPosition).normalized * _speed;
        }
    }
}