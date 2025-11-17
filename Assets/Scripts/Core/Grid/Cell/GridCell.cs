using Core.Grid.Cell.Abstract;
using UnityEngine;

namespace Core.Grid.Cell
{
    /// <summary>
    /// Represents a single cell within a grid system, inheriting functionality and structure
    /// from the <see cref="CellBase"/> base class.
    /// </summary>
    /// <remarks>
    /// This class provides specific behavior for grid cells such as positioning,
    /// highlighting, and concealing functionalities. It ensures that the visual representation
    /// of the cell is properly managed during its lifecycle.
    /// </remarks>
    public class GridCell : CellBase
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer visual;
        [Header("VFX")]
        [SerializeField] private Color highlightColor = Color.green;

        public override Vector3 Position => transform.position;

        private bool _isAwaken;
        
        protected override void OnAwake()
        {
            if (visual == null)
            {
                _isAwaken = false;
                Debug.LogError("Visual is null");
                return;
            }
            
            _isAwaken = true;
        }

        public override void Highlight()
        {
            if (!_isAwaken)
            {
                Debug.LogError("Visual is not awaken");
                return;
            }
            visual.color = highlightColor;
        }

        public override void Conceal()
        {
            if (!_isAwaken)
            {
                Debug.LogError("Visual is not awaken");
                return;
            }
            visual.color = Color.white;
        }
    }
}