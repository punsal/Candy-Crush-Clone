using Core.Camera.Provider.Abstract;
using UnityEngine;

namespace Core.Camera.Provider
{
    /// <summary>
    /// Manages and provides access to a Camera instance within a Unity scene,
    /// serving as a concrete implementation of the ACameraReference class.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class UnityCameraProvider : UnityCameraProviderBase
    {
        private UnityEngine.Camera _camera;

        public override UnityEngine.Camera Instance => _camera;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            if (_camera != null)
            {
                return;
            }
        
            Debug.LogWarning("No camera assigned to CameraReference, will search for main camera");
            _camera = UnityEngine.Camera.main;
        }
    }
}