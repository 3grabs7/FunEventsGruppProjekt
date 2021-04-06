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
using Microsoft.AspNetCore.Authorization;

namespace FunEvents.Pages.Events
{
    [Authorize]
    public class MyEventsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public MyEventsModel(ApplicationDbContext context,
           UserManager<AppUser> userManager,
           SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public AppUser AppUser { get; set; }
        public List<Event> Events { get; set; }
        public bool RemovingEventFailed { get; set; }
        public bool RemovingEventSucceeded { get; set; }

        public async Task OnGetAsync(
            bool? removingEventFailed,
            bool? removingEventSucceeded)
        {

            RemovingEventFailed = removingEventFailed ?? false;
            RemovingEventSucceeded = removingEventSucceeded ?? false;

            string userId = _userManager.GetUserId(User);

            AppUser = await _context.Users
                .Where(u => u.Id == userId)
                .Include(u => u.JoinedEvents)
                .FirstOrDefaultAsync();

            Events = await _context.Events
                .Where(e => e.Attendees
                .Contains(AppUser))
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                AppUser = await _context.Users
                    .Where(u => u.Id == userId)
                    .Include(u => u.JoinedEvents)
                    .FirstOrDefaultAsync();

                Event eventToRemove = await _context.Events
                    .Where(e => e.Id == id)
                    .FirstOrDefaultAsync();

                AppUser.JoinedEvents.Remove(eventToRemove);
                eventToRemove.SpotsAvailable++;

                await _context.SaveChangesAsync();
            }
            catch
            {
                return RedirectToPage("/Events/MyEvents", new { RemovingEventFailed = true });
            }

            return RedirectToPage("/Events/MyEvents", new { RemovingEventSucceeded = true });
        }
    }
}
