using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using Microsoft.EntityFrameworkCore;

namespace HSU_Recommender_system.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;

        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UniversityRecommendationResult>> GetRecommendationsAsync(string userId)
        {
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var results = new List<UniversityRecommendationResult>();
            
            if (profile == null) return results;

           
            var programs = await _context.AcademicPrograms
                .Include(p => p.University)
                .ToListAsync();

            foreach (var program in programs)
            {
                if (program.University == null) continue;

                double cgpaRatio = (profile.CGPA / program.MinCGPA) * 100;
                double ieltsRatio = (profile.EnglishProficiencyScore / program.MinIELTS) * 100;
                double admissionChance = Math.Min(100, (cgpaRatio * 0.7) + (ieltsRatio * 0.3));

                decimal totalCost = program.TotalTuitionFee + program.University.EstimatedLivingCost;
                double budgetFit = profile.MaximumBudget >= totalCost 
                    ? 100 
                    : (double)(profile.MaximumBudget/totalCost)*100;

                double raBoost = (profile.NumberOfResearchPapers * 12) + (profile.NumberOfProjects * 4);
                double taBoost = (profile.WorkExperienceMonths * 1.5) + (profile.NumberOfProjects * 5);
                
                double raChance = Math.Min(100, program.HistoricalRAChance + raBoost);
                double taChance = Math.Min(100, program.HistoricalTAChance + taBoost);

                double overallScore = (admissionChance * 0.45) + (budgetFit * 0.25) + (raChance * 0.15) + (taChance * 0.15);

                string category;
                string reason;

                if (admissionChance >= 90 && budgetFit >= 80)
                {
                    category = "Safe";
                    reason = "Your scores significantly exceed requirements and it fits your budget perfectly.";
                }
                else if (overallScore >= 80)
                {
                    category = "Best Fit";
                    reason = "Strong academic alignment and highly favorable RA/TA funding opportunities.";
                }
                else if (overallScore >= 65)
                {
                    category = "Target";
                    reason = "You meet the standard requirements, but assistantships will be competitive.";
                }
                else
                {
                    category = "Ambitious";
                    reason = "Your profile is below historical averages for this program; admission is a reach.";
                }

                results.Add(new UniversityRecommendationResult
                {
                    University = program.University,
                    Program = program,
                    AdmissionChance = Math.Round(admissionChance, 1),
                    BudgetFit = Math.Round(budgetFit, 1),
                    RaChance = Math.Round(raChance, 1),
                    TaChance = Math.Round(taChance, 1),
                    OverallScore = Math.Round(overallScore, 1),
                    Category = category,
                    RecommendationReason = reason
                });
            }

            if (!string.IsNullOrEmpty(profile.PreferredCountry))
            {
                results = results.Where(r => r.University!.Country == profile.PreferredCountry).ToList();
            }

            return results.OrderByDescending(r => r.OverallScore).ToList();
        }
    }
}

