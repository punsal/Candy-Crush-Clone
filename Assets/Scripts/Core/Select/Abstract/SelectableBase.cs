using Core.Grid.Cell.Interface;
using Core.Select.Interface;
using UnityEngine;

namespace Core.Select.Abstract
{
    /// <summary>
    /// Represents the base class for a selectable element in the game, providing core functionality
    /// for interaction with grid cells and selection mechanics. Inherits from MonoBehaviour and
    /// implements the ISelectable interface.
    /// </summary>
    public abstract class SelectableBase : MonoBehaviour, ISelectable
    {
        public ICell Cell { get; private set; }
        public string Name => name;

        private bool _isSelected;

        public void Occupy(ICell cell)
        {
            if (Cell != null)
            {
                Debug.LogError($"Selectable({Name}) already has a cell({cell.Name})");
                return;
            }
        
            Cell = cell;
            
            OnOccupied(cell);
        }

        protected virtual void OnOccupied(ICell cell)
        {
            Debug.Log($"{name} occupied the cell {cell.Name}");
        }
        
        public void Release()
        {
            PreRelease(Cell);
            Cell = null;
        }

        protected virtual void PreRelease(ICell cell)
        {
            var cellName = cell == null ? "null" : cell.Name;
            Debug.Log($"{name} released the cell {cellName}");
        }
        
        public void Select()
        {
            if (_isSelected)
            {
                return;
            }

            _isSelected = true;
            Cell?.Highlight();
            
            OnSelected();
            
            Debug.Log($"Selected {Name} at [{Cell?.Row:00}, {Cell?.Column:00}]");
        }

        protected abstract void OnSelected();

        public void Unselect()
        {
            if (!_isSelected)
            {
                return;
            }
            
            _isSelected = false;
            Cell?.Conceal();
            
            OnUnselected();
            
            Debug.Log($"Unselected {Name} at [{Cell?.Row:00}, {Cell?.Column:00}]");
        }
        
        protected abstract void OnUnselected();
        public abstract bool IsAdjacent(ISelectable other);
    }
}