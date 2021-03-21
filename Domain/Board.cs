using System.Collections.Generic;

namespace Domain
{
    public class Board
    {
        public int BoardId { get; set; }
        
        // public string? BoardState { get; set; } // json
        public int GameId { get; set; }
        public Game Game { get; set; } = null!;
        public ICollection<Ship> Ships { get; set; } = null!;
    }
}