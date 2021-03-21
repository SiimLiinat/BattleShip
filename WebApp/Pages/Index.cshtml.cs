using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Console = Colorful.Console;
using Ship = GameBrain.Ship;

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;

        public IndexModel(DAL.ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
        }
        
        public IList<Game> Game { get; set; }  = default!;
        [BindProperty] public int PlayerA { get; set; } = default!;
        [BindProperty] public int PlayerB { get; set; } = default!;
        [BindProperty] public int ENextMoveAfterHit { get; set; } = default!;
        [BindProperty] public int EShipsCanTouch { get; set; }
        [BindProperty] public int BoardSize { get; set; }
        [BindProperty] public string? ShipStandards { get; set; }

        public async Task OnGetAsync()
        {
            Game = await _context
                .Games
                .Include(game => game.Players)
                .OrderBy(x => x.CreatedAt).ToListAsync();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            GameBrain.Game battleship = new();
            battleship.SetBoardSize(BoardSize, BoardSize);
            battleship.EPlayerType1 = (EPlayerType) PlayerA;
            battleship.EPlayerType2 = (EPlayerType) PlayerB;
            battleship.EShipsCanTouch = (EShipsCanTouch) EShipsCanTouch;
            battleship.ENextMoveAfterHit = (ENextMoveAfterHit) ENextMoveAfterHit;
            if (ShipStandards != null)
            {
                var shipStandards = JsonSerializer.Deserialize<Dictionary<int, int>>(ShipStandards);
                if (shipStandards != null)
                {
                    battleship.SetShipStandards(shipStandards);
                }
            }
            var gameId = await battleship.SaveGameToDataBase();
            return RedirectToPage("./PlaceShips/Index/", new { id = gameId, side = 1});
        }
    }
}