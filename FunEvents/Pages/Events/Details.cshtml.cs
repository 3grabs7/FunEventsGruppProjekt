using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FunEvents.Data;
using FunEvents.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FunEvents.Pages.Events
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public DetailsModel(ApplicationDbContext context,
           UserManager<AppUser> userManager,
           SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public Event EventToJoin { get; set; }
        public AppUser AppUser { get; set; }
        public bool SucceededToJoinEvent { get; set; }
        public bool FailedToJoinEvent { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id,
            bool? succeededToJoinEvent,
            bool? failedToJoinEvent)
        {
            if (id == null)
            {
                return RedirectToPage("/Errors/NotFound");
            }

            SucceededToJoinEvent = succeededToJoinEvent ?? false;
            FailedToJoinEvent = failedToJoinEvent ?? false;

            EventToJoin = await _context.Events.FindAsync(id);
            string userId = _userManager.GetUserId(User);
            AppUser = await _context.Users.Where(u => u.Id == userId).Include(u => u.JoinedEvents).FirstOrDefaultAsync();

            if (AppUser == default)
            {
                return RedirectToPage("/Errors/NotFound");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return RedirectToPage("/Errors/NotFound");
            }

            EventToJoin = await _context.Events.FindAsync(id);

            string userId = _userManager.GetUserId(User);
            AppUser appUser = await _context.Users.Where(u => u.Id == userId).Include(u => u.JoinedEvents).FirstOrDefaultAsync();

            try
            {
                appUser.JoinedEvents.Add(EventToJoin);
                EventToJoin.SpotsAvailable--;

                await _context.SaveChangesAsync();
            }
            catch
            {
                return RedirectToPage("/Events/Details", new { id = id, failedToJoinEvent = true });
            }

            return RedirectToPage("/Events/Details", new { id = id, succeededToJoinEvent = true });
        }

        public int AttendeesCount() => _context.Events
            .Include(e => e.Attendees)
            .Where(e => e.Id == EventToJoin.Id)
            ?.First().Attendees.Count
            ?? 0;

        public string GetAttendeeInfo()
        {
            var attendees = _context.Events
                .Include(e => e.Attendees)
                .Where(e => e.Id == EventToJoin.Id)
                .First().Attendees;

            string output = String.Join('\n', attendees?.Select(a => a.UserName)
                ?? new List<string> { "Couldn't Load Users" });

            return output;
        }

    }
}
