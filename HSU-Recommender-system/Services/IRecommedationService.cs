using HSU_Recommender_system.Models;

namespace HSU_Recommender_system.Services
{
    public interface IRecommendationService
    {
        Task<List<UniversityRecommendationResult>> GetRecommendationsAsync(string userId);
    }
}

