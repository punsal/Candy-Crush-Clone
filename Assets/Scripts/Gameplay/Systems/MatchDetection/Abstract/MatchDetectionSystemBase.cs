using System.Collections.Generic;
using Core.Grid.Abstract;
using Gameplay.Tile.Abstract;

namespace Gameplay.Systems.MatchDetection.Abstract
{
    /// <summary>
    /// Serves as the base class for systems responsible for detecting matches
    /// and identifying possible moves within a grid-based board game.
    /// </summary>
    public abstract class MatchDetectionSystemBase
    {
        protected readonly TileManagerBase TileManager;
        protected readonly GridSystemBase GridSystem;

        protected MatchDetectionSystemBase(GridSystemBase gridSystem, TileManagerBase tileManager)
        {
            GridSystem = gridSystem;
            TileManager = tileManager;
        }

        public abstract bool HasPossibleMoves();
        public abstract bool TryGetMatch(out List<TileBase> tiles);
    }
}