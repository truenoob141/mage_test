using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Gui.Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using Zenject;
using Object = UnityEngine.Object;

namespace MageTest.Gui
{
    public abstract class SimplePresenter<T> : IInitializable, IDisposable, IPresenter
        where T : MonoBehaviour, IWindow
    {
        [Inject]
        private readonly DiContainer _diContainer;
        [Inject]
        private readonly GameSettings _gameSettings;
        [Inject]
        private readonly GuiController _guiController;

        public bool IsShown { get; private set; }
        public bool IsLoading { get; private set; }
        public bool IsClosing { get; private set; }

        public virtual UIFlagsEnum Flags => UIFlagsEnum.None;
        public abstract UILayerEnum Layer { get; }

        protected T view;

        private AssetReferenceGameObject _assetRef;
        private CancellationTokenSource _cts;

        public void Initialize()
        {
            _cts = new CancellationTokenSource();
            
            var assets = _gameSettings._viewAssetRefCollection;
            if (assets == null)
                throw new Exception($"Failed to initialize presenter {GetType().FullName}: No view assets");

            _assetRef = assets.GetReference<T>();
        }

        public void Dispose()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
            }
        }

        public virtual void CloseWindow(bool force = false)
        {
            Assert.IsNotNull(view, $"{GetType().Name} hasn't a view");
            Assert.IsNotNull(_assetRef, $"{GetType().Name} hasn't a asset reference");

            try
            {
                IsClosing = true;

                if (IsShown)
                    HideWindow();

                if (view != null)
                {
                    try
                    {
                        _assetRef.ReleaseInstance(view.gameObject);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to release instance " + view.name);
                        Debug.LogException(ex);
                        
                        Object.Destroy(view.gameObject);
                    }
                    
                    view = null;
                }

                // FIXME Clean code
                _guiController.RemoveIndex(this);
            }
            finally
            {
                IsClosing = false;
            }
        }

        public virtual void ShowWindow()
        {
            Assert.IsNotNull(view, "No window");

            IsShown = true;

            var go = view.gameObject;
            go.SetActive(true);
            OnShow();
        }

        public virtual void HideWindow()
        {
            Assert.IsNotNull(view, "No window");

            IsShown = false;

            OnHide();
            view.gameObject.SetActive(false);
        }
        
        public Transform GetViewTransform()
        {
            return view != null ? view.transform : null;
        }

        protected virtual UniTask OnLoading(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        protected UniTask<bool> LoadAndShowWindow(CancellationToken token)
        {
            return LoadWindow(token)
                .ContinueWith(success =>
                {
                    if (success)
                        ShowWindow();

                    return success;
                });
        }

        protected async UniTask<bool> LoadWindow(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            string key = typeof(T).Name;

            // Multi touch
            if (IsLoading || view != null)
            {
                Debug.Log($"Failed to load window '{key}': Already loading");
                return false;
            }

            Assert.IsNull(view, "Already window " + key);

            IsLoading = true;
            bool hasThrobber = (Flags & UIFlagsEnum.NoThrobber) == 0;
            if (hasThrobber)
                _guiController.SetThrobber(this);

            token.Register(() => _guiController.RemoveThrobber(this));

            var parent = _guiController.GetRoot(Layer);
            try
            {
                var root = _guiController.GetDisableRoot();
                view = await InstantiateWindowNoInject(root, token);
                if (view == null)
                {
                    Debug.LogError($"Failed to load window: Failed to load view of type {typeof(T).Name}");
                }
                else
                {
                    var go = view.gameObject;
                    _diContainer.InjectGameObject(go);
                    go.SetActive(false);
                    go.transform.SetParent(parent, false);

                    await OnLoading(token);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load window {typeof(T).Name}");
                Debug.LogException(ex);
            }
            finally
            {
                if (hasThrobber)
                    _guiController.RemoveThrobber(this);

                IsLoading = false;
            }

            return view != null;
        }

        protected CancellationToken GetDisposeToken()
        {
            return _cts?.Token ?? new CancellationToken(true);
        }

        UniTask<bool> IPresenter.LoadWindow(CancellationToken token)
        {
            return LoadWindow(token);
        }

        private async UniTask<T> InstantiateWindowNoInject(Transform parent, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var assets = _gameSettings._viewAssetRefCollection;
            if (assets == null)
            {
                Debug.LogError("Failed to instantiate window: No view assets");
                return null;
            }

            var assetReference = assets.GetReference<T>();
            if (assetReference == null)
            {
                Debug.LogError($"Failed to instantiate window: Window '{typeof(T)}' not found");
                return null;
            }

            var op = assetReference.InstantiateAsync(parent);
            var go = await op.Task;
            token.ThrowIfCancellationRequested();
            
            // FIXME https://github.com/Cysharp/UniTask/issues/573
            // var go = await assetReference.InstantiateAsync(parent)
            //     .ToUniTask(cancellationToken: token);
            
            if (token.IsCancellationRequested)
            {
                if (go != null)
                {
                    try
                    {
                        _assetRef.ReleaseInstance(go);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to release instance " + view.name);
                        Debug.LogException(ex);
                        
                        Object.Destroy(go);
                    }
                }

                token.ThrowIfCancellationRequested();
            }

            var component = go.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"GameObject '{go.name}' hasn't {typeof(T).Name}");

                try
                {
                    _assetRef.ReleaseInstance(go);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed to release instance " + view.name);
                    Debug.LogException(ex);
                    
                    Object.Destroy(go);
                }
            }

            return component;
        }
    }
}