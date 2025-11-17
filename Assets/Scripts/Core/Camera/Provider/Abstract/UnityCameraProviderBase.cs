using Core.Camera.Provider.Interface;
using UnityEngine;

namespace Core.Camera.Provider.Abstract
{
    /// <summary>
    /// Serves as an abstraction layer for referencing and managing a Camera instance
    /// within the Unity engine, providing mechanisms for derived classes to implement
    /// and extend camera-related functionality.
    /// </summary>
    public abstract class UnityCameraProviderBase : MonoBehaviour, ICameraProvider
    {
        public abstract UnityEngine.Camera Instance { get; }
    }
}