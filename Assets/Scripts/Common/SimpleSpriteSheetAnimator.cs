using System;
using UnityEngine;

namespace MageTest.Common
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleSpriteSheetAnimator : MonoBehaviour
    {
        [SerializeField]
        private float _delayBetweenFrames = 0.1f;
        [SerializeField]
        private Sprite[] _sprites;

        private SpriteRenderer _spriteRenderer;
        private float _startTime;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _startTime = Time.time;
        }

        private void Update()
        {
            float totalTime = _sprites.Length * _delayBetweenFrames;
            int index = (int) (((Time.time - _startTime) % totalTime) / _delayBetweenFrames);
            _spriteRenderer.sprite = _sprites[index];
        }
    }
}