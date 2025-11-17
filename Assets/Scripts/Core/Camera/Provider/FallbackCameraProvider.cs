using Core.Camera.Provider.Interface;

namespace Core.Camera.Provider
{
    /// <summary>
    /// Provides a fallback implementation of the <see cref="ICameraProvider"/> interface,
    /// which uses a pre-defined Unity camera instance as the source.
    /// </summary>
    /// <remarks>
    /// This class is typically utilized when no specific camera provider is available,
    /// and the default Unity main camera (or another fallback camera) should be used.
    /// </remarks>
    public class FallbackCameraProvider : ICameraProvider
    {
        public UnityEngine.Camera Instance { get; }

        public FallbackCameraProvider(UnityEngine.Camera instance)
        {
            Instance = instance;
        }
    }
}