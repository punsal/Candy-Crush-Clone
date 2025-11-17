namespace Core.Provider.Interface
{
    /// <summary>
    /// Represents a generic provider interface for getting an instance of a specified type.
    /// </summary>
    /// <typeparam name="T">The type of the instance provided.</typeparam>
    public interface IProvider<out T>
    {
        T Instance { get; }
    }
}