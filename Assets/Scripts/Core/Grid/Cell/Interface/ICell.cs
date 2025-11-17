using UnityEngine;

namespace Core.Grid.Cell.Interface
{
    /// <summary>
    /// Defines the basic structure and behavior for a grid cell.
    /// </summary>
    /// <remarks>
    /// This interface is used to represent individual cells in a grid system.
    /// It includes properties for the cell's name, position, row, and column
    /// in the grid, as well as methods for initialization, destruction,
    /// highlighting, and concealing the cell.
    /// </remarks>
    public interface ICell
    {
        string Name { get; }
        int Row { get; }
        int Column { get; }
        Vector3 Position { get; }
        void Initialize(int row, int column);
        void Destroy();
        void Highlight();
        void Conceal();
    }
}