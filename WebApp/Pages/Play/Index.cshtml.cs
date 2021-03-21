using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Enums;
using GameBrain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Board = GameBrain.Board;
using Game = Domain.Game;
using Ship = Domain.Ship;

namespace WebApp.Pages.Play
{
    public class IndexModel : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;

        public IndexModel(DAL.ApplicationDbContext context)
        {
            _context = context;
        }

        public Game? Game { get; set; }
        public GameBrain.Game BattleShip { get; set; } = new();
        public bool Continue { get; set; }
        public Board Board1 { get; set; } = default!;
        public Board Board2 { get; set; } = default!;
        
        public Player Player { get; set; } = default!;
        public ShipCoordinate? ShipCoordinate { get; set; }
        public Ship? Ship { get; set; }
        public bool GameOver { get; set; }

        public async Task OnGetAsync(int id, int? x, int ?y, bool cont = false)
        {
            Continue = cont;
            Game = await _context.Games
                .Where(game => game.GameId == id)
                .Include(g => g.GameOption)
                .Include(g => g.Players)
                .FirstOrDefaultAsync();
            GameOver = Game.GameOver;
            if (Game?.GameJson != null)
            {
                BattleShip.SetGameStateFromJsonString(Game.GameJson);
                BattleShip.EShipsCanTouch = Game.GameOption!.EShipsCanTouch;
                BattleShip.ENextMoveAfterHit = Game.GameOption!.ENextMoveAfterHit;
            }
            switch (Game!.TurnA)
            {
                case true:
                    Board1 = BattleShip.Board2;
                    Board2 = BattleShip.Board1;
                    Player = Game.Players[0];
                    break;
                case false:
                    Board1 = BattleShip.Board1;
                    Board2 = BattleShip.Board2;
                    Player = Game.Players[1];
                    break;
            }

            if (!Continue && Player.EPlayerType == EPlayerType.AI)
            {
                Continue = true;
                (x, y) = AI.MakeStatisticalMove(Board1, null);
            }
            
            if (x.HasValue && y.HasValue && !GameOver)
            {
                var (canMakeMove, isAHit) = Board1.MakeMove(BattleShip, x.Value, y.Value);

                if (canMakeMove && isAHit)
                {
                    ShipCoordinate = await _context.ShipCoordinates
                        .Where(sc => sc.X == x && sc.Y == y)
                        .FirstOrDefaultAsync();
                    if (ShipCoordinate != null)
                    {
                        _context.ShipCoordinates.Remove(ShipCoordinate);
                        Ship = await _context.Ships
                            .Where(s => s.ShipId == ShipCoordinate.ShipId)
                            .Include(s => s.ShipCoordinates)
                            .FirstOrDefaultAsync();
                        await _context.SaveChangesAsync();
                        if (Ship?.ShipCoordinates?.Count == 0)
                        {
                            Ship.IsSunken = true;
                        }
                    }

                    if (BattleShip.ENextMoveAfterHit == ENextMoveAfterHit.OtherPlayer) Game.TurnA = !Game.TurnA;
                }
                else if (canMakeMove) Game.TurnA = !Game.TurnA;

                if (Board1.GetShipSquares().Count == 0)
                {
                    Game!.GameOver = true;
                    GameOver = true;
                }

                Game!.GameJson = BattleShip.GetSerializedGameState();
                await _context.SaveChangesAsync();
            }
        }
    }
}