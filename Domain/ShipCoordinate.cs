namespace Domain
{
    public class ShipCoordinate
    {
        public int ShipCoordinateId { get; set; }
        public int X { get; set; } = default!;
        public int Y { get; set; } = default!;
        public int ShipId { get; set; }
        public Ship Ship { get; set; } = null!;
    }
}