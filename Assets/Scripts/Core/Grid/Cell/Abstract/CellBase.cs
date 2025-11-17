using Core.Grid.Cell.Interface;
using UnityEngine;

namespace Core.Grid.Cell.Abstract
{
    /// <summary>
    /// Represents the base functionality for a grid cell in the system, providing common properties
    /// and methods shared by all cell types.
    /// </summary>
    /// <remarks>
    /// This is an abstract class that serves as a foundation for all grid-based cells. It defines
    /// key properties like position, row, and column and enforces the implementation of highlight
    /// and conceal behaviors.
    /// </remarks>
    public abstract class CellBase : MonoBehaviour, ICell
    {
        public string Name => _isDestroyed ? string.Empty : name;
        public int Row { get; private set; }
        public int Column { get; private set; }

        private bool _isDestroyed;
        public abstract Vector3 Position { get; }

        private void Awake()
        {
            _isDestroyed = false;
            OnAwake();
        }
        
        protected abstract void OnAwake();

        private void OnDestroy()
        {
            _isDestroyed = true;
        }

        public void Initialize(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public void Destroy()
        {
            if (!_isDestroyed)
            {
                Destroy(gameObject);
            }
        }
    
        public abstract void Highlight();
        public abstract void Conceal();
    }
}