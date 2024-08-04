using System;
using System.Collections.Generic;
using System.Linq;
using MageTest.Gui.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MageTest.Gui
{
    public class GuiController : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private GameObject _throbber;
        [SerializeField]
        private GameObject _fade;
        [SerializeField]
        private LayerObject[] _layers;

        private Transform _disabledContainer;

        private readonly Dictionary<UILayerEnum, Dictionary<IPresenter, int>> _presenterIndices = new();
        private readonly List<IPresenter> _stash = new();
        private readonly Dictionary<UILayerEnum, List<IPresenter>> _presenters = new();
        private readonly List<object> _throbberLockers = new();

        private readonly UILayerEnum[] _ignoreLayers = { UILayerEnum.Debug };

        private bool _isDirty;

        private void Awake()
        {
            _throbber.SetActive(false);
            _fade.SetActive(false);
        }

        private void LateUpdate()
        {
            // Apply
            if (!_isDirty)
                return;

            _isDirty = false;

            _fade.transform.SetParent(_disabledContainer, false);

            foreach (var dict in _presenterIndices.Values)
            {
                foreach ((var presenter, int index) in dict.OrderBy(kv => kv.Value))
                {
                    var view = presenter.GetViewTransform();
                    if (view != null)
                        view.SetSiblingIndex(index);
                }
            }

            UpdateFade();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject != _fade)
                return;

            var presenter = GetCurrentPresenter(UIFlagsEnum.HasFade);
            if (presenter == null || (presenter.Flags & UIFlagsEnum.FadeClickIsClose) == 0)
                return;

            if (!presenter.IsClosing)
            {
                presenter.CloseWindow();
                RemoveIndex(presenter);
            }
        }

        public Transform GetDisableRoot()
        {
            if (_disabledContainer == null)
            {
                var go = new GameObject("Disabled");
                go.SetActive(false);
                go.transform.SetParent(transform, false);

                var rectTransform = go.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.pivot = Vector2.one * 0.5f;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;

                _disabledContainer = rectTransform;
                return _disabledContainer;
            }

            if (_disabledContainer.gameObject.activeSelf)
                _disabledContainer.gameObject.SetActive(false);

            return _disabledContainer;
        }

        public void SetThrobber(object locker)
        {
            _throbberLockers.Add(locker);

            if (_throbber != null)
                _throbber.SetActive(true);
        }

        public void RemoveThrobber(object locker)
        {
            _throbberLockers.Remove(locker);

            if (_throbberLockers.Count == 0 && _throbber != null && _throbber.activeSelf)
                _throbber.SetActive(false);
        }

        public void RemoveIndex(IPresenter presenter)
        {
            var layer = presenter.Layer;

            Dictionary<IPresenter, int> indices;
            if (!_presenterIndices.TryGetValue(layer, out indices))
                return;

            indices.Remove(presenter);

            if (indices.Count == 0)
                _presenterIndices.Remove(layer);
        }

        public void RegisterPresenter(IPresenter presenter)
        {
            var layer = presenter.Layer;

            List<IPresenter> list;
            if (!_presenters.TryGetValue(layer, out list))
            {
                list = new List<IPresenter>(1);
                _presenters.Add(layer, list);
            }

            var root = GetRoot(layer);
            if (root == null)
                throw new ArgumentException("Layer not found: " + layer);

            SetIndex(presenter);
            list.Add(presenter);

            if (IsAutoStash(presenter))
                foreach (var toStash in GetAutoStashPresenters(presenter))
                    StashPresenter(toStash);

            UpdateFlags();
        }

        public void UnregisterPresenter(IPresenter presenter)
        {
            var layer = presenter.Layer;

            List<IPresenter> list;
            if (!_presenters.TryGetValue(layer, out list))
                throw new ArgumentException($"Failed to unregister presenter {presenter}: Layer {layer} not found");

            bool removed = list.Remove(presenter);
            if (!removed)
                throw new ArgumentException($"Failed to unregister presenter {presenter}: Can't remove from the list");

            if (IsAutoStash(presenter))
            {
                var current = GetCurrentPresenterExcept(UIFlagsEnum.DisableAutoStash,
                    kv => kv.Key < UILayerEnum.Popups);

                if (current == null)
                {
                    var stashed = GetLastStashedPresenter();
                    if (stashed != null)
                        UnStashPresenter(stashed);
                }
            }

            UpdateFlags();
        }

        public Transform GetRoot(UILayerEnum layer)
        {
            var lo = _layers.FirstOrDefault(l => l._layer == layer);
            return lo?._gameObject.transform;
        }

        public IPresenter GetCurrentPresenter()
        {
            return _presenters.Where(kv => !_ignoreLayers.Contains(kv.Key)).OrderByDescending(kv => kv.Key)
                .Select(kv => kv.Value).FirstOrDefault(l => l.Count > 0)?.LastOrDefault();
        }

        public IPresenter GetCurrentPresenter(UIFlagsEnum flags)
        {
            return _presenters.Values
                .SelectMany(p => p)
                .OrderBy(p => p.Layer)
                .ThenBy(p => _presenterIndices[p.Layer][p])
                .LastOrDefault(p => (p.Flags & flags) != 0);
        }

        public IPresenter GetCurrentPresenterExcept(UIFlagsEnum flags)
        {
            return _presenters
                .Where(kv => !_ignoreLayers.Contains(kv.Key))
                .OrderBy(kv => kv.Key)
                .SelectMany(kv => kv.Value)
                .LastOrDefault(p => (p.Flags & flags) == 0);
        }

        public bool HasOverlay(IPresenter presenter)
        {
            var first = _presenters
                .Where(kv => !_ignoreLayers.Contains(kv.Key))
                .OrderBy(kv => kv.Key)
                .SelectMany(kv => kv.Value)
                .LastOrDefault();

            return first != presenter;
        }

        public void Close(UILayerEnum layer)
        {
            Debug.Log("Close " + layer);

            var closed = new List<IPresenter>(_stash);
            foreach (var presenter in _stash.Where(p => p.Layer == layer).ToArray())
            {
                presenter.CloseWindow();
                _stash.Remove(presenter);

                RemoveIndex(presenter);
            }

            foreach (var group in _presenters.Where(kv => kv.Key == layer).OrderBy(kv => kv.Key)
                         .SelectMany(kv => kv.Value).GroupBy(p => p.Layer).ToArray())
            {
                foreach (var presenter in group)
                {
                    presenter.CloseWindow();
                    RemoveIndex(presenter);
                }

                _presenters.Remove(group.Key);
                closed.AddRange(group);
            }

            UpdateFlags();
        }

        public List<IPresenter> CloseAll(params UILayerEnum[] except)
        {
            return CloseAll(false, except);
        }

        public List<IPresenter> CloseAll(bool force, params UILayerEnum[] except)
        {
            Debug.Log("Close all except " + string.Join(", ", except));

            var closed = new List<IPresenter>(_stash);
            foreach (var presenter in _stash.Where(p => !except.Contains(p.Layer) &&
                         !_ignoreLayers.Contains(p.Layer)).ToArray())
            {
                presenter.CloseWindow(force);
                _stash.Remove(presenter);

                RemoveIndex(presenter);
            }

            foreach (var group in _presenters.Where(kv => !except.Contains(kv.Key) && !_ignoreLayers.Contains(kv.Key))
                         .OrderByDescending(kv => kv.Key).SelectMany(kv => kv.Value).GroupBy(p => p.Layer).ToArray())
            {
                foreach (var presenter in group.Reverse())
                {
                    presenter.CloseWindow(force);
                    RemoveIndex(presenter);
                }

                _presenters.Remove(group.Key);
                closed.AddRange(group);
            }

            UpdateFlags();
            return closed;
        }

        public IPresenter GetCurrentPresenterExcept(
            UIFlagsEnum flags,
            Func<KeyValuePair<UILayerEnum, List<IPresenter>>, bool> predicate)
        {
            return _presenters.Where(kv => !_ignoreLayers.Contains(kv.Key) && predicate(kv)).OrderBy(kv => kv.Key)
                .SelectMany(kv => kv.Value).LastOrDefault(p => (p.Flags & flags) == 0);
        }

        private void SetIndex(IPresenter presenter)
        {
            var layer = presenter.Layer;

            Dictionary<IPresenter, int> indices;
            if (!_presenterIndices.TryGetValue(layer, out indices))
            {
                indices = new Dictionary<IPresenter, int>();
                _presenterIndices.Add(layer, indices);
            }

            int index;
            if (!indices.TryGetValue(presenter, out index))
            {
                index = indices.Count > 0 ? indices.Values.Last() + 1 : 0;
                indices.Add(presenter, index);
            }

            var trans = presenter.GetViewTransform();
            if (trans != null)
                trans.SetSiblingIndex(index);
        }

        private void StashPresenter(IPresenter presenter)
        {
            presenter.HideWindow();

            _stash.Add(presenter);
            UpdateFlags();
        }

        private void UnStashPresenter(IPresenter presenter, bool showWindow = true)
        {
            bool removed = _stash.Remove(presenter);
            if (!removed)
                throw new ArgumentException($"Failed to unstash presenter {presenter}: Can't remove from the list");

            if (showWindow)
                presenter.ShowWindow();

            UpdateFlags();
        }

        private IPresenter GetLastStashedPresenter()
        {
            return _stash.Where(IsAutoStash).Reverse().FirstOrDefault();
        }

        private IPresenter[] GetAutoStashPresenters(params IPresenter[] except)
        {
            return _presenters.Where(kv => kv.Key < UILayerEnum.Popups).OrderBy(kv => kv.Key).SelectMany(kv => kv.Value)
                .Where(p => (p.Flags & UIFlagsEnum.DisableAutoStash) == 0).Except(except).ToArray();
        }

        private void UpdateFlags()
        {
            _isDirty = true;
        }

        private void UpdateFade()
        {
            _fade.SetActive(false);

            var presenter = GetCurrentPresenter(UIFlagsEnum.HasFade);
            if (presenter == null)
                return;

            var root = GetRoot(presenter.Layer);
            _fade.transform.SetParent(root);
            _fade.transform.SetAsLastSibling();

            var view = presenter.GetViewTransform();
            _fade.transform.SetSiblingIndex(view.GetSiblingIndex());
            _fade.SetActive(true);
        }

        private bool IsAutoStash(IPresenter presenter)
        {
            return presenter.Layer < UILayerEnum.Popups && (presenter.Flags & UIFlagsEnum.DisableAutoStash) == 0;
        }

        [Serializable]
        private class LayerObject
        {
            public UILayerEnum _layer = UILayerEnum.None;
            public GameObject _gameObject;
        }
    }
}