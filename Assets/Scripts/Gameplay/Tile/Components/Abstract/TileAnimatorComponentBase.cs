using UnityEngine;

namespace Gameplay.Tile.Components.Abstract
{
    /// <summary>
    /// Provides a base class for handling animations related to tile components in the game.
    /// This abstract class defines core animation functionalities such as selecting, unselecting,
    /// destruction, and movement, to be implemented by derived classes.
    /// </summary>
    public abstract class TileAnimatorComponentBase : MonoBehaviour
    {
        public abstract void PlaySelectEffect();
        public abstract void PlayUnselectEffect();
        public abstract void AnimateDestruction();
        public abstract void AnimateMovement(Vector3 target, float duration);
    }
}