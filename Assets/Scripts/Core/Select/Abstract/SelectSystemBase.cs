using System;
using Core.Camera.Provider.Interface;
using Core.Select.Interface;
using UnityEngine;

namespace Core.Select.Abstract
{
    /// <summary>
    /// Serves as an abstract base class for implementing selection systems.
    /// Provides core functionality related to object selection, including the
    /// ability to handle dragging operations and tracking selection states.
    /// </summary>
    public abstract class SelectSystemBase : IDisposable
    {
        protected UnityEngine.Camera Camera { get; private set; }
        protected LayerMask LayerMask { get; private set; }
        public abstract event Action<ISelectable, ISelectable> OnSelectionCompleted;
        public bool IsDragging { get; protected set; }

        protected SelectSystemBase(ICameraProvider cameraProvider, LayerMask layerMask)
        {
            Camera = cameraProvider.Instance;
            LayerMask = layerMask;
        }
    
        public abstract void Dispose();
        public abstract void StartDrag();
        public abstract void UpdateDrag();
        public abstract void EndDrag();
    }
}