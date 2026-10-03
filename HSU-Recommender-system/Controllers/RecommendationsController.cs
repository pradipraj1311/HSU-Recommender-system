using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using HSU_Recommender_system.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HSU_Recommender_system.Controllers
{
    
    [Authorize(Roles = "Student")]
    public class RecommendationsController : Controller
    {
        private readonly IRecommendationService _recommendationEngine;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecommendationsController(IRecommendationService recommendationEngine, UserManager<ApplicationUser> userManager)
        {
            _recommendationEngine = recommendationEngine;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Authentication required.");

          
            var recommendations = await _recommendationEngine.GetRecommendationsAsync(user.Id);

            return View(recommendations);
        }
    }
}