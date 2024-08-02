using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Gui;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using Zenject;

namespace MageTest.PoolSystem
{
    public abstract class BasePool : MonoBehaviour
    {
        public void Despawn(IEnumerable instances)
        {
            foreach (object instance in instances)
                Despawn(instance);
        }

        public abstract void Despawn(object instance);
    }

    public abstract class BasePool<T> : BasePool where T : Component
    {
        [Inject]
        private readonly DiContainer _container;
        [Inject]
        private readonly GuiController _guiController;
        [SerializeField]
        private AssetReference _assetReference;

        private readonly Stack<T> _inactives = new();

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
            // Disable game object
            instance.gameObject.SetActive(false);

            // Back to inactivity pool
            _inactives.Push(instance);
        }

        protected async UniTask<T[]> SpawnItems(int amount, CancellationToken token)
        {
            var views = new T[amount];
            var tasks = new UniTask[amount];
            for (int i = 0; i < tasks.Length; ++i)
            {
                int index = i;
                tasks[i] = Spawn(token)
                    .ContinueWith(v => views[index] = v);
            }

            try
            {
                await UniTask.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                foreach (var view in views.Where(v => v != null))
                    Destroy(view.gameObject);

                throw;
            }

            foreach (var view in views)
                view.transform.SetAsLastSibling();

            return views;
        }

        protected UniTask<T> Spawn(CancellationToken token)
        {
            return Spawn(true, token);
        }

        protected async UniTask<T> Spawn(bool setParentAndActive, CancellationToken token)
        {
            Assert.IsNotNull(_assetReference);
            Assert.IsTrue(_assetReference.RuntimeKeyIsValid());

            // Get available instance
            T instance;
            do
            {
                if (_inactives.Count == 0)
                {
                    var disabled = _guiController.GetDisableRoot();
                    var go = await Addressables.InstantiateAsync(_assetReference, disabled)
                        .ToUniTask(cancellationToken: token);

                    instance = go.GetComponent<T>();
                    if (instance == null)
                    {
                        Destroy(go);
                        throw new Exception($"Failed to spawn item: '{_assetReference}' hasn't {typeof(T)}");
                    }

                    _container.InjectGameObject(go);

                    if (setParentAndActive)
                        go.transform.SetParent(transform, false);
                }
                else
                {
                    instance = _inactives.Pop();
                }
            }
            while (instance == null);

            if (setParentAndActive)
                instance.gameObject.SetActive(true);

            return instance;
        }
    }
}