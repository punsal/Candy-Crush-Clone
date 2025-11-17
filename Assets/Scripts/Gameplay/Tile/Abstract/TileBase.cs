using Core.Select.Abstract;
using Gameplay.Systems.MatchDetection.Type;
using Gameplay.Tile.Components.Abstract;
using UnityEngine;

namespace Gameplay.Tile.Abstract
{
    /// <summary>
    /// Represents the base class for all tile objects in the game. This class provides common functionality for tiles,
    /// including selection, movement, animation handling, and destruction. All tiles in the system should inherit
    /// from this abstract class and implement specific behaviors as required.
    /// </summary>
    public abstract class TileBase : SelectableBase
    {
        [Header("Components")]
        [SerializeField] private TileAnimatorComponentBase animator;
        
        public abstract MatchType MatchType { get; set; }

        private void Awake()
        {
            OnAwake();
        }

        protected virtual void OnAwake()
        {
            if (animator == null)
            {
                Debug.LogError("Animator component not assigned");
            }
        }

        public abstract bool IsTypeMatch(MatchType type);

        protected override void OnSelected()
        {
            animator?.PlaySelectEffect();
        }

        protected override void OnUnselected()
        {
            animator?.PlayUnselectEffect();
        }

        public void Destroy()
        {
            animator?.AnimateDestruction();
        }
        
        public void MoveTo(Vector3 position, float duration)
        {
            animator.AnimateMovement(position, duration);
        }
    }
}