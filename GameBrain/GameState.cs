using System.Collections.Generic;

namespace GameBrain
{
    public class GameState
    {
        public bool NextMoveByPlayer1 { get; set; }
        public CellState[][] Board1 { get; set; } = null!;
        public CellState[][] Board2 { get; set; } = null!;
        
        public Dictionary<int, int>? Ships { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}