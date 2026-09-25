using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HSU_Recommender_system.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagementController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var viewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    IsAdmin = await _userManager.IsInRoleAsync(user, "Admin")
                };
                userRolesViewModel.Add(viewModel);
            }

            return View(userRolesViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                    if (adminUsers.Count > 1)
                    {
                        await _userManager.RemoveFromRoleAsync(user, "Admin");
                        await _userManager.AddToRoleAsync(user, "Student");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Action Denied: You cannot demote the last remaining Admin in the system.";
                    }
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, "Student");
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}