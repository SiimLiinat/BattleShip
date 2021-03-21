using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.PlaceShip
{
    public class IndexModel : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;

        public IndexModel(DAL.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty] public EShipPositioning EShipPositioning { get; set; }
        public Game Game { get; set; } = default!;
        [BindProperty] public int Id { get; set; }
        public GameBrain.Game BattleShip { get; set; } = new();
        public GameBrain.Board Board { get; set; } = default!;
        public IList<Ship> Ships { get; set; } = new List<Ship>();
        public int Length { get; set; }
        [BindProperty] public int Side { get; set; }

        public async Task<IActionResult> OnPostAsync ()
        {
            return RedirectToPage("../PlaceShip/Index",
                new {id = Id, side = Side, eShipPositioning = EShipPositioning});
        }
        public async Task OnGetAsync(int id, int side, EShipPositioning? eShipPositioning)
        {
            Game = await _context.Games
                .Where(game => game.GameId == id)
                .Include(g => g.Boards)
                .Include(g => g.GameOption)
                .FirstOrDefaultAsync();
            Side = side;
            if (eShipPositioning != null)
            {
                EShipPositioning = eShipPositioning.Value;
            }
            if (Game.GameJson != null)
            {
                BattleShip.SetGameStateFromJsonString(Game.GameJson);
                BattleShip.EShipsCanTouch = Game.GameOption!.EShipsCanTouch;
                BattleShip.ENextMoveAfterHit = Game.GameOption!.ENextMoveAfterHit;
            }
            Board = side == 1 ? BattleShip.Board1 : BattleShip.Board2;
            var domainBoard = side == 1 ? Game.Boards[0] : Game.Boards[1];

            foreach (var (key, value) in BattleShip.GetShipStandards())
            {
                Ships = await _context.Ships
                    .Where(ship => ship.BoardId == domainBoard.BoardId && ship.Size == key)
                    .ToListAsync();
                if (Ships.Count != value)
                {
                    Length = key;
                    break;
                }
            }
        }
    }
}