using Core.Camera.Provider.Interface;

namespace Core.Camera.Abstract
{
    /// <summary>
    /// Represents the base functionality for camera systems within the application.
    /// Provides a foundation for managing camera operations and ensuring proper
    /// positioning of the camera relative to game elements.
    /// </summary>
    public abstract class CameraSystemBase
    {
        private readonly ICameraProvider _cameraProvider;
        protected UnityEngine.Camera Camera => _cameraProvider.Instance;
        
        protected CameraSystemBase(ICameraProvider cameraProvider)
        {
            _cameraProvider = cameraProvider;
        }
        
        public abstract void CenterOnGrid(int rows, int columns);
    }
}