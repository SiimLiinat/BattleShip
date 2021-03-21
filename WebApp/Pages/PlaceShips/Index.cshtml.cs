using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Enums;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Board = GameBrain.Board;
using Game = Domain.Game;

namespace WebApp.Pages.PlaceShips
{
    public class IndexModel : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;

        public IndexModel(DAL.ApplicationDbContext context)
        {
            _context = context;
        }

        public Game Game { get; set; } = default!;
        public Board Board { get; set; } = default!;
        public GameBrain.Game BattleShip { get; set; } = new();
        private IList<Domain.Ship> Ships { get; set; } = new List<Domain.Ship>();
        private EPlayerType? EPlayerType { get; set; }
        
        public async Task<IActionResult> OnGetAsync(int id, int? x, int ?y, int side, int? length, EShipPositioning? eShipPositioning, bool random = false)
        {
            Game = await _context.Games
                .Where(game => game.GameId == id)
                .Include(g => g.GameOption)
                .Include(g => g.Players)
                .Include(g => g.Boards)
                .FirstOrDefaultAsync();
            
            if (side == 1)
            {
                Ships = await _context.Ships
                    .Where(g => g.BoardId == Game.Boards[0].BoardId).ToListAsync();
            }
            if (side == 2)
            {
                Ships = await _context.Ships
                    .Where(g => g.BoardId == Game.Boards[1].BoardId).ToListAsync();
            }
            
            if (Game == null) return RedirectToPage("./Index/");
            
            if (Game.GameJson != null)
            {
                BattleShip.SetGameStateFromJsonString(Game.GameJson);
            }

            Board = side == 1 ? BattleShip.Board1 : BattleShip.Board2;
            EPlayerType = side == 1 ? Game!.Players[0].EPlayerType : Game!.Players[1].EPlayerType;
            
            var gameShip = new GameBrain.Ship();
            if (x.HasValue && y.HasValue && eShipPositioning.HasValue && length.HasValue)
            {
                gameShip = GameBrain.Ship.AddShip(Board, x.Value, y.Value, (int) eShipPositioning.Value,length!.Value, Game!.GameOption!.EShipsCanTouch);
            }
            else if (random && length.HasValue)
            {
                gameShip = GameBrain.Ship.AddRandomShip(Board, length!.Value,Game!.GameOption!.EShipsCanTouch);
            }
            
            if (gameShip != null && gameShip.Coordinates.Count != 0)
            {
                Domain.Ship domainShip = new()
                {
                    Name = gameShip.Length + "x1",
                    Size = gameShip.Length,
                    Board = side == 1 ? Game.Boards[0] : Game.Boards[1],
                    ShipCoordinates = new List<ShipCoordinate>()
                };
                foreach (var (item1, item2) in gameShip.Coordinates)
                {
                    ShipCoordinate shipCoordinate = new()
                    {
                        X = item1,
                        Y = item2,
                        Ship = domainShip
                    };
                    await _context.ShipCoordinates.AddAsync(shipCoordinate);
                    domainShip.ShipCoordinates.Add(shipCoordinate);
                    Board.board[item1, item2] = CellState.S;
                }
                await _context.Ships.AddAsync(domainShip);
                Game.GameJson = BattleShip.GetSerializedGameState();
                await _context.SaveChangesAsync();
            }
            
            if (Ships.Count == BattleShip.GetShipCount())
            {
                if (side == 1)
                {
                    return RedirectToPage("../PlaceShips/Index/", new { id = Game.GameId, side = 2});
                }
                else
                {
                    return RedirectToPage("../Play/Index/", new { id = Game!.GameId});
                }
            }

            if (EPlayerType == Domain.Enums.EPlayerType.AI)
            {
                var ships = AI.AddRandomShips(BattleShip, Board, Game!.GameOption!.EShipsCanTouch);
                if (ships.Count == BattleShip.GetShipCount())
                {
                    foreach (var ship in ships)
                    {
                        Domain.Ship domainShip = new()
                        {
                            Name = ship.Length + "x1",
                            Size = ship.Length,
                            Board = side == 1 ? Game.Boards[0] : Game.Boards[1],
                            ShipCoordinates = new List<ShipCoordinate>()
                        };
                        foreach (var (item1, item2) in ship.Coordinates)
                        {
                            ShipCoordinate shipCoordinate = new()
                            {
                                X = item1,
                                Y = item2,
                                Ship = domainShip
                            };
                            await _context.ShipCoordinates.AddAsync(shipCoordinate);
                            domainShip.ShipCoordinates.Add(shipCoordinate);
                            Board.board[item1, item2] = CellState.S;
                        }

                        await _context.Ships.AddAsync(domainShip);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            else return RedirectToPage("../PlaceShip/Index/", new { id = Game.GameId, side});

            Game.GameJson = BattleShip.GetSerializedGameState();
            await _context.SaveChangesAsync();
            return RedirectToPage("../PlaceShips/Index/", new { id = Game.GameId, side});
        }
    }
}