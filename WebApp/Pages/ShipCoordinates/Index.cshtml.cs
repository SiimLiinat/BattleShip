using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain;

namespace WebApp.Pages_ShipCoordinates
{
    public class IndexModel : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;

        public IndexModel(DAL.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ShipCoordinate> ShipCoordinate { get; set; } = default!;

        public async Task OnGetAsync()
        {
            ShipCoordinate = await _context.ShipCoordinates
                .Include(s => s.Ship).ToListAsync();
        }
    }
}
