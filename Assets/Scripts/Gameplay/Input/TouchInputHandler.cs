using System;
using Core.Select.Abstract;
using Gameplay.Input.Abstract;
using UnityEngine;

namespace Gameplay.Input
{
    /// <summary>
    /// Handles touch-based input within the gameplay system, interpreting touch gestures such as drag-and-drop.
    /// Inherits from the <see cref="InputHandlerBase"/> class to provide platform-specific input processing
    /// for touch-enabled devices.
    /// </summary>
    /// <remarks>
    /// This class utilizes a <see cref="SelectSystemBase"/> implementation to manage object selection
    /// and manipulation during touch interactions. It processes single-touch inputs and determines the
    /// interactions (e.g., starting drag, updating drag, or ending drag) based on touch phases.
    /// </remarks>
    public class TouchInputHandler : InputHandlerBase
    {
        public TouchInputHandler(SelectSystemBase selectSystem) : base(selectSystem)
        {
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            
            // If input is disabled while dragging, end the drag
            if (SelectSystem.IsDragging)
            {
                SelectSystem.EndDrag();
            }
        }

        protected override void ProcessInput()
        {
            if (UnityEngine.Input.touchCount == 0)
            {
                return;
            }

            // single finger
            var touch = UnityEngine.Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    SelectSystem.StartDrag();
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (SelectSystem.IsDragging)
                    {
                        SelectSystem.UpdateDrag();
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (SelectSystem.IsDragging)
                    {
                        SelectSystem.EndDrag();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Platform input-state is not supported");
            }
        }
    }
}