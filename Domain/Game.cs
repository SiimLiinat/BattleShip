using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Game
    {
        public int GameId { get; set; }
        public int GameOptionId { get; set; }
        public GameOption? GameOption { get; set; }
        public string? GameJson { get; set; }
        public bool GameOver { get; set; } = false;
        public string CreatedAt { get; set; } = DateTime.Now.ToLongDateString();
        [MaxLength(2)] public IList<Player> Players { get; set; } = null!;
        [MaxLength(2)] public IList<Board> Boards { get; set; } = null!;
        public bool TurnA { get; set; } = true;
    }
}