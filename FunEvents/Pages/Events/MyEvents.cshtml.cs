using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FunEvents.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FunEvents.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FunEvents.Pages.Events
{
    public class MyEventsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ActiveUser> _userManager;
        private readonly SignInManager<ActiveUser> _signInManager;

        public MyEventsModel(ApplicationDbContext context,
           UserManager<ActiveUser> userManager,
           SignInManager<ActiveUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public ActiveUser ActiveUser { get; set; }

        public List<Event> Events { get; set; }

        public async Task OnGetAsync()
        {
            string userId = _userManager.GetUserId(User);

            ActiveUser = await _context.Users.Where(u => u.Id == userId).Include(u => u.MyEvents).FirstOrDefaultAsync();

            Events = await _context.Events.Where(e => e.Attendees.Contains(ActiveUser)).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                // Something went wrong here with routing correct event to remove
            }

            string userId = _userManager.GetUserId(User);
            ActiveUser = await _context.Users.Where(u => u.Id == userId).Include(u => u.MyEvents).FirstOrDefaultAsync();
            Event eventToRemove = await _context.Events.Where(e => e.Id == id).FirstOrDefaultAsync();
            ActiveUser.MyEvents.Remove(eventToRemove);
            eventToRemove.SpotsAvailable++;
            await _context.SaveChangesAsync();

            return Page();
        }
    }
}
