using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Gui;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace MageTest.PoolSystem
{
    public abstract class DynamicPool<T> : BasePool where T : Component
    {
        [Inject]
        private readonly DiContainer _container;
        [Inject]
        private readonly GuiController _guiController;
        
        private readonly Dictionary<T, string> _actives = new();
        private readonly Dictionary<string, Stack<T>> _inactives = new();

        public sealed override void Despawn(object instance)
        {
            Despawn((T) instance);
        }

        public void Despawn(IEnumerable<T> instances)
        {
            foreach (var instance in instances)
                Despawn(instance);
        }

        public virtual void Despawn(T instance)
        {
            if (instance == null)
                return;

            // Remove from active
            if (!_actives.Remove(instance, out string prefabId))
                throw new ArgumentException("Failed to despawn item: Unknown instance " + instance);

            // Disable game object
            instance.gameObject.SetActive(false);

            // Back to inactivity pool
            Stack<T> inactives;
            if (!_inactives.TryGetValue(prefabId, out inactives))
            {
                inactives = new Stack<T>();
                _inactives.Add(prefabId, inactives);
            }

            inactives.Push(instance);
        }

        protected UniTask<T> Spawn(AssetReferenceGameObject assetRef, CancellationToken token)
        {
            return Spawn(assetRef, true, token);
        }
        
        protected async UniTask<T> Spawn(
            AssetReferenceGameObject assetRef, bool setParentAndActivate, CancellationToken token)
        {
            if (assetRef == null || !assetRef.RuntimeKeyIsValid())
                throw new ArgumentException("Failed to spawn item: Prefab not found");

            T instance;
            string prefabId = assetRef.RuntimeKey.ToString();

            Stack<T> inactives;
            if (_inactives.TryGetValue(prefabId, out inactives) && inactives.Count > 0)
            {
                instance = inactives.Pop();
                _actives.Add(instance, prefabId);
            }
            else
            {
                var disabled = _guiController.GetDisableRoot();
                var go = await Addressables.InstantiateAsync(assetRef, disabled)
                    .ToUniTask(cancellationToken: token);

                instance = go.GetComponent<T>();
                if (instance == null)
                {
                    Destroy(go);
                    throw new Exception($"Failed to spawn item: '{assetRef}' hasn't {typeof(T)}");
                }

                _actives.Add(instance, prefabId);

                Inject(instance);

                if (setParentAndActivate)
                    go.transform.SetParent(transform, false);
            }

            if (setParentAndActivate)
                instance.gameObject.SetActive(true);

            return instance;
        }

        protected virtual void Inject(T component)
        {
            _container.InjectGameObject(component.gameObject);
        }
    }
}