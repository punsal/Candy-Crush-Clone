using Core.Grid.Cell.Interface;

namespace Core.Grid.Interface
{
    /// <summary>
    /// Represents an object that can occupy a cell on a grid.
    /// </summary>
    public interface ICellOccupant
    {
        ICell Cell { get; }
        void Occupy(ICell cell);
        void Release();
    }
}