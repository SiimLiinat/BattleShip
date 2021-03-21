using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain
{
    public class GameOption
    {
        public int GameOptionId { get; set; }
        
        [MaxLength(128)] public string? Name { get; set; }

        public int BoardWidth { get; set; }
        public int BoardLength { get; set; }
        
        // enums are saved as ints in database
        public EShipsCanTouch EShipsCanTouch { get; set; }
        public ENextMoveAfterHit ENextMoveAfterHit { get; set; }
        
        public ICollection<Game>? Games { get; set; }
    }
}