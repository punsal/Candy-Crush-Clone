using Core.Grid.Interface;

namespace Core.Select.Interface
{
    /// <summary>
    /// Defines an interface for objects that can be selected, unselected,
    /// and checked for adjacency with other selectable objects.
    /// Inherits from the <see cref="ICellOccupant"/> interface.
    /// </summary>
    public interface ISelectable : ICellOccupant
    {
        string Name { get; }
        void Select();
        void Unselect();
        bool IsAdjacent(ISelectable other);
    }
}