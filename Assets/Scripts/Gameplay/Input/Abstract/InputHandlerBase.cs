using Core.Select.Abstract;
using UnityEngine;

namespace Gameplay.Input.Abstract
{
    /// <summary>
    /// An abstract base class that provides the structure for input handling within the gameplay system.
    /// Defines the fundamental lifecycle of input handling, including enabling, disabling, and processing input.
    /// </summary>
    public abstract class InputHandlerBase
    {
        protected readonly SelectSystemBase SelectSystem;
        private bool IsEnabled { get; set; }

        protected InputHandlerBase(SelectSystemBase selectSystem)
        {
            SelectSystem = selectSystem;
            IsEnabled = false;
        }

        public void Enable()
        {
            IsEnabled = true;
            OnEnabled();
        }

        protected virtual void OnEnabled()
        {
            Debug.Log("Input enabled");
        }

        public void Disable()
        {
            IsEnabled = false;
            OnDisabled();
        }

        protected virtual void OnDisabled()
        {
            Debug.Log("Input disabled");
        }

        public void Update()
        {
            if (!IsEnabled)
            {
                return;
            }

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            ProcessInput();
        }
        
        protected abstract void ProcessInput();
    }
}