using Core.Select.Abstract;
using Core.Select.Interface;
using Gameplay.Systems.MatchDetection.Type;
using Gameplay.Tile.Abstract;
using UnityEngine;

namespace Gameplay.Tile
{
    /// <summary>
    /// Represents a tile that can be selected and interacts with the game board.
    /// Inherits from <see cref="SelectableBase"/> to provide behavior for selecting
    /// and adjacency checks specific to a chip.
    /// </summary>
    public class BasicTile : TileBase
    {
        [Header("Match")]
        [SerializeField] private MatchType matchType;
        public override MatchType MatchType
        {
            get => matchType;
            set => matchType = value;
        }

        public override bool IsTypeMatch(MatchType type)
        {
            if (matchType == MatchType.Any)
            {
                return true;
            }
            
            if (type == MatchType.Any)
            {
                return true;
            }
            
            return type == MatchType;
        }

        public override bool IsAdjacent(ISelectable other)
        {
            if (Cell == null)
            {
                Debug.LogWarning("Tile is null");
                return false;
            }

            if (other == null)
            {
                Debug.LogWarning("Other is null");
                return false;
            }

            if (other.Cell == null)
            {
                Debug.LogWarning("Other tile is null");
                return false;
            }

            var rowDiff = Mathf.Abs(Cell.Row - other.Cell.Row);
            var columnDiff = Mathf.Abs(Cell.Column - other.Cell.Column);

            // force horizontal or vertical
            return (rowDiff == 1 && columnDiff == 0) || (rowDiff == 0 && columnDiff == 1);
        }
    }
}