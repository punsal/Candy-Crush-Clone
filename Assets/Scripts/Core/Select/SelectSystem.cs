using System;
using Core.Camera.Provider.Interface;
using Core.Select.Abstract;
using Core.Select.Interface;
using UnityEngine;

namespace Core.Select
{
    /// <summary>
    /// A system for managing object selection and drag interactions within a Unity environment.
    /// This class allows users to select objects using input mechanisms (such as mouse or touch)
    /// and provides functionality to detect and process drag interactions between selectable objects.
    /// </summary>
    public class SelectSystem : SelectSystemBase
    {
        private event Action<ISelectable, ISelectable> onSelectionCompleted;

        public override event Action<ISelectable, ISelectable> OnSelectionCompleted
        {
            add => onSelectionCompleted += value;
            remove => onSelectionCompleted -= value;
        }

        private ISelectable _firstSelectable;
        private ISelectable _lastSelectable;

        public SelectSystem(ICameraProvider cameraProvider, LayerMask layerMask) : base(cameraProvider, layerMask)
        {
            IsDragging = false;
        }

        public override void Dispose()
        {
            onSelectionCompleted = null;
        }

        public override void StartDrag()
        {
            var selectable = GetSelectableAtInputPosition();

            if (selectable == null)
            {
                Debug.LogWarning("No selectable found, won't start drag");
                return;
            }

            IsDragging = true;
            _firstSelectable = selectable;
            _firstSelectable.Select();
        }

        public override void UpdateDrag()
        {
            var selectable = GetSelectableAtInputPosition();
            if (selectable == null)
            {
                // assume player will find a selectable in the future
                return;
            }

            if (selectable == _lastSelectable)
            {
                // still waiting, ignore
                return;
            }

            // did player go back to first selectable? (undo logic/deselect last)
            if (selectable == _firstSelectable && _lastSelectable != null)
            {
                _lastSelectable.Unselect();
                _lastSelectable = null;
                return;
            }

            // check if the selectable is adjacent to the last selectable
            if (!IsSelectable(selectable))
            {
                return;
            }
            _lastSelectable?.Unselect();
            _lastSelectable = selectable;
            _lastSelectable.Select();
        }

        public override void EndDrag()
        {
            IsDragging = false;
            
            _firstSelectable?.Unselect();
            _lastSelectable?.Unselect();
            
            // check
            if (_firstSelectable != null && _lastSelectable != null)
            {
                onSelectionCompleted?.Invoke(_firstSelectable, _lastSelectable);
            }

            ClearSelectables();
        }

        private void ClearSelectables()
        {
            _firstSelectable = null;
            _lastSelectable = null;
        }

        private ISelectable GetSelectableAtInputPosition()
        {
            var position = Camera.ScreenToWorldPoint(Input.mousePosition);
            var hit2D = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, LayerMask);

            return hit2D.collider 
                ? hit2D.collider.GetComponent<ISelectable>() 
                : null;
        }

        private bool IsSelectable(ISelectable selectable)
        {
            if (selectable == null)
            {
                Debug.LogWarning("Selectable cannot be null");
                return false;
            }

            if (_firstSelectable != null)
            {
                return _firstSelectable.IsAdjacent(selectable);
            }

            Debug.LogWarning("No first selectable to compare with, cannot check adjacency.");
            return false;
        }
    }
}