using Core.Select.Abstract;
using Gameplay.Input.Abstract;

namespace Gameplay.Input
{
    /// <summary>
    /// Handles mouse-based input for interaction with the game environment by extending the
    /// functionality of the abstract InputHandlerBase class. It manages user input through
    /// mouse events and translates these interactions into dragging operations on the associated
    /// selection system.
    /// </summary>
    /// <remarks>
    /// MouseInputHandler is primarily responsible for detecting and processing mouse button
    /// states, such as pressing, holding, and releasing the left mouse button, and appropriately
    /// invoking the StartDrag, UpdateDrag, and EndDrag methods on the SelectSystem object.
    /// </remarks>
    public class MouseInputHandler : InputHandlerBase
    {
        public MouseInputHandler(SelectSystemBase selectSystem) : base(selectSystem)
        {
            // empty
        } 
        
        protected override void OnDisabled()
        {
            base.OnDisabled();
            
            // caution to force end drag
            if (SelectSystem.IsDragging)
            {
                SelectSystem.EndDrag();
            }
        }

        protected override void ProcessInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                SelectSystem.StartDrag();
            }
            else if (UnityEngine.Input.GetMouseButton(0) && SelectSystem.IsDragging)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                SelectSystem.UpdateDrag();
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0) && SelectSystem.IsDragging)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                SelectSystem.EndDrag();
            }
        }
    }
}