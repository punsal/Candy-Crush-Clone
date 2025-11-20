using System.Collections.Generic;
using Core.Grid.Abstract;
using Gameplay.Systems.MatchDetection.Abstract;
using Gameplay.Systems.MatchDetection.Type;
using Gameplay.Tile.Abstract;
using Unity.Profiling;

namespace Gameplay.Systems.MatchDetection
{
    /// <summary>
    /// The MatchDetectionSystem class is responsible for identifying matches and detecting valid moves
    /// within a grid-based puzzle game (e.g., Candy Crush style). It extends the MatchDetectionSystemBase
    /// and provides implementations for detecting tile matches and validating possible moves.
    /// </summary>
    public class MatchDetectionSystem : MatchDetectionSystemBase
    {
        // ProfileMarkers for measuring performance
        private static readonly ProfilerMarker TryGetMatchMarker = new("MatchDetection.TryGetMatch");
        private static readonly ProfilerMarker HasPossibleMovesMarker = new("MatchDetection.HasPossibleMoves");

        // Reusable collections to avoid allocations
        private readonly List<TileBase> _matchBuffer;
        private readonly List<TileBase> _runBuffer;
        private readonly HashSet<TileBase> _seenTiles;

        
        public MatchDetectionSystem(GridSystemBase gridSystem, TileManagerBase tileManager) : base(gridSystem, tileManager)
        {
            _matchBuffer = new List<TileBase>(gridSystem.RowCount * gridSystem.ColumnCount);
            _runBuffer = new List<TileBase>(12);
            _seenTiles = new HashSet<TileBase>();
        }

        /// <summary>
        /// Detects matches and valid swap moves on the board in a Candy-Crush style:
        /// - Matches are horizontal or vertical lines of 3+ tiles of the same type.
        /// - A "possible move" is a swap between two adjacent tiles that would create such a line.
        /// </summary>
        public override bool TryGetMatch(out List<TileBase> tiles)
        {
            TryGetMatchMarker.Begin();
            
            _matchBuffer.Clear();
            _seenTiles.Clear();

            var rowCount = GridSystem.RowCount;
            var columnCount = GridSystem.ColumnCount;

            // Horizontal matches
            for (var row = 0; row < rowCount; row++)
            {
                var currentRow = row;
                CollectLineMatches(
                    length: columnCount,
                    getTileAtIndex: col => TileManager.FindTileAt(currentRow, col),
                    _matchBuffer
                );
            }

            // Vertical matches
            for (var col = 0; col < columnCount; col++)
            {
                var currentColumn = col;
                CollectLineMatches(
                    length: rowCount,
                    getTileAtIndex: row => TileManager.FindTileAt(row, currentColumn),
                    _matchBuffer
                );
            }

            // Remove duplicates (tiles that belong to both a row and a column match)
            tiles = new List<TileBase>(_matchBuffer.Count);
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var tile in _matchBuffer)
            {
                if (!_seenTiles.Add(tile))
                {
                    continue;
                }
                
                tiles.Add(tile);
            }
            
            var hasMatch = tiles.Count >= 3;
            TryGetMatchMarker.End();
            
            return hasMatch;
        }
        
        /// <summary>
        /// Returns true if there exists at least one swap between adjacent tiles
        /// that would create a horizontal or vertical line of 3+ tiles.
        /// </summary>
        public override bool HasPossibleMoves()
        {
            HasPossibleMovesMarker.Begin();
            
            var rowCount = GridSystem.RowCount;
            var columnCount = GridSystem.ColumnCount;

            for (var row = 0; row < rowCount; row++)
            {
                for (var col = 0; col < columnCount; col++)
                {
                    var tile = TileManager.FindTileAt(row, col);
                    if (tile == null || tile.Cell == null)
                    {
                        continue;
                    }

                    // Try right neighbor
                    if (col + 1 < columnCount)
                    {
                        var right = TileManager.FindTileAt(row, col + 1);
                        if (right != null && right.Cell != null && WouldSwapCreateMatch(tile, right))
                        {
                            HasPossibleMovesMarker.End();
                            return true;
                        }
                    }

                    // Try down neighbor
                    if (row + 1 < rowCount)
                    {
                        var down = TileManager.FindTileAt(row + 1, col);
                        if (down != null && down.Cell != null && WouldSwapCreateMatch(tile, down))
                        {
                            HasPossibleMovesMarker.End();
                            return true;
                        }
                    }
                }
            }

            HasPossibleMovesMarker.End();
            return false;
        }
        
        /// <summary>
        /// Collects all tiles that are part of horizontal/vertical runs of 3+,
        /// within a single line (either row or column).
        /// </summary>
        private void CollectLineMatches(int length, System.Func<int, TileBase> getTileAtIndex, List<TileBase> accumulator)
        {
            MatchType? currentType = null;
            _runBuffer.Clear();

            void FlushRun()
            {
                if (_runBuffer.Count >= 3)
                {
                    accumulator.AddRange(_runBuffer);
                }

                _runBuffer.Clear();
                currentType = null;
            }

            for (var i = 0; i < length; i++)
            {
                var tile = getTileAtIndex(i);

                if (tile == null)
                {
                    FlushRun();
                    continue;
                }

                if (currentType == null || tile.MatchType != currentType.Value)
                {
                    FlushRun();
                    currentType = tile.MatchType;
                    _runBuffer.Add(tile);
                }
                else
                {
                    _runBuffer.Add(tile);
                }
            }

            // flush at the end of line
            FlushRun();
        }
        
        /// <summary>
        /// Temporarily swaps the TileType of two tiles, checks if that would create
        /// a match at either position, then swaps back.
        /// </summary>
        private bool WouldSwapCreateMatch(TileBase a, TileBase b)
        {
            // Safety checks
            if (a == null || b == null || a.Cell == null || b.Cell == null)
            {
                return false;
            }

            // Swap types
            var typeA = a.MatchType;
            var typeB = b.MatchType;
            a.MatchType = typeB;
            b.MatchType = typeA;

            var rowA = a.Cell.Row;
            var colA = a.Cell.Column;
            var rowB = b.Cell.Row;
            var colB = b.Cell.Column;

            var result =
                HasLineMatchAt(rowA, colA) ||
                HasLineMatchAt(rowB, colB);

            // Revert swap
            a.MatchType = typeA;
            b.MatchType = typeB;

            return result;
        }
        
        /// <summary>
        /// Checks whether the tile at (row, col) is part of a horizontal OR vertical
        /// run of 3+ tiles of the same type.
        /// </summary>
        private bool HasLineMatchAt(int row, int col)
        {
            var center = TileManager.FindTileAt(row, col);
            if (center == null)
            {
                return false;
            }

            var targetType = center.MatchType;

            // Horizontal count
            var horizontalCount = 1;

            // left
            horizontalCount += CountDirection(row, col, 0, -1, targetType);
            // right
            horizontalCount += CountDirection(row, col, 0, 1, targetType);

            if (horizontalCount >= 3)
            {
                return true;
            }

            // Vertical count
            var verticalCount = 1;

            // up
            verticalCount += CountDirection(row, col, -1, 0, targetType);
            // down
            verticalCount += CountDirection(row, col, 1, 0, targetType);

            return verticalCount >= 3;
        }
        
        private int CountDirection(int startRow, int startCol, int dRow, int dCol, MatchType targetType)
        {
            var rowCount = GridSystem.RowCount;
            var columnCount = GridSystem.ColumnCount;

            var count = 0;
            var row = startRow + dRow;
            var col = startCol + dCol;

            while (row >= 0 && row < rowCount && col >= 0 && col < columnCount)
            {
                var tile = TileManager.FindTileAt(row, col);
                if (tile == null || tile.MatchType != targetType)
                {
                    break;
                }

                count++;
                row += dRow;
                col += dCol;
            }

            return count;
        }
        
    }
}