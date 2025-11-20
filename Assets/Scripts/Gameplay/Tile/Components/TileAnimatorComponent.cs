using System.Collections;
using Gameplay.Tile.Components.Abstract;
using UnityEngine;

namespace Gameplay.Tile.Components
{
    /// <summary>
    /// Handles animations related to tile behavior, including selecting, unselecting,
    /// destruction, and movement. This class extends from TileAnimatorComponentBase
    /// and provides concrete implementations for animation effects.
    /// </summary>
    public class TileAnimatorComponent : TileAnimatorComponentBase
    {
        [Header("Settings")]
        [SerializeField] private float selectScaleMultiplier = 1.2f;
        [SerializeField] private float destroyDuration = 0.2f;
        [SerializeField] private float moveDuration = 0.2f;
        
        private Vector3 _originalScale;
        private Coroutine _currentAnimation;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        public override void PlaySelectEffect()
        {
            StopCurrentAnimation();
            transform.localScale = _originalScale * selectScaleMultiplier;
        }

        public override void PlayUnselectEffect()
        {
            StopCurrentAnimation();
            transform.localScale = _originalScale;
        }

        public override void AnimateDestruction()
        {
            StopCurrentAnimation();
            _currentAnimation = StartCoroutine(ScaleDownAnimation(destroyDuration));
        }

        public override void AnimateMovement(Vector3 target, float duration)
        {
            StopCurrentAnimation();
            _currentAnimation = StartCoroutine(MoveAnimation(target, moveDuration));
        }

        public override void ResetState()
        {
            StopCurrentAnimation();

            transform.localScale = _originalScale != Vector3.zero 
                ? _originalScale 
                : Vector3.one;
        }

        private IEnumerator ScaleDownAnimation(float duration)
        {
            var startScale = transform.localScale;
            var elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                
                yield return null;
            }
        }

        private IEnumerator MoveAnimation(Vector3 target, float duration)
        {
            var startPosition = transform.position;
            var elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                transform.position = Vector3.Lerp(startPosition, target, t);
                
                yield return null;
            }
        }

        private void StopCurrentAnimation()
        {
            if (_currentAnimation == null)
            {
                return;
            }
            StopCoroutine(_currentAnimation);
            _currentAnimation = null;
        }
    }
}