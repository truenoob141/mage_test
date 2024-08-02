using UnityEngine;

namespace MageTest.Gui
{
    public class GuiController : MonoBehaviour
    {
        private Transform _disabledContainer;
        
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
    }
}