using Core.Provider.Interface;

namespace Core.Camera.Provider.Interface
{
    /// <summary>
    /// Defines an interface for providing instances of Unity camera objects.
    /// </summary>
    /// <remarks>
    /// This interface is used to abstract the retrieval of a specific Unity camera instance,
    /// allowing for different implementations of camera provisioning. It extends the generic
    /// provider interface, `IProvider`, tailored for `UnityEngine.Camera`.
    /// </remarks>
    public interface ICameraProvider : IProvider<UnityEngine.Camera>
    {
        
    }
}