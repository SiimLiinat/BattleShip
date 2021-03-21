using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Ship
    {
        public int ShipId { get; set; }
        [MaxLength(10)] public string? Name { get; set; } // 5x1 etc
        [Range(1, 50)] public int Size { get; set; }
        public bool IsSunken { get; set; } = false;
        public IList<ShipCoordinate>? ShipCoordinates { get; set; }
        public int BoardId { get; set; }
        public Board Board { get; set; } = null!;
    }
}