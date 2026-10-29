using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            // 1. Determine if the student wants research
            bool isResearchTrack = !string.IsNullOrWhiteSpace(profile.ResearchInterests) && profile.ResearchInterests != "None";

            // EVERYTHING BELOW MUST BE INSIDE THIS LOOP
            foreach (var program in programs)
            {
                if (program.University == null) continue;

                // Re-added the missing ratio calculations
                double cgpaRatio = (profile.CGPA / Math.Max(1, program.MinCGPA)) * 100;
                double ieltsRatio = (profile.EnglishProficiencyScore / Math.Max(1, program.MinIELTS)) * 100;

                // 2. Calculate scores
                double basicAcademicScore = Math.Min(100, (cgpaRatio * 0.7) + (ieltsRatio * 0.3));

                double greScore = 100;
                if (program.IsGRERequired && program.TargetGREQuant > 0)
                {
                    greScore = ((double)profile.GREQuantScore / program.TargetGREQuant) * 100;
                    greScore = Math.Min(100, greScore);
                }

                double researchFitScore = isResearchTrack ? CalculateResearchFit(profile.ResearchInterests, program.ResearchKeywords) : 0;

                if (isResearchTrack && program.University.CarnegieClassification != null && program.University.CarnegieClassification.Contains("R1"))
                {
                    researchFitScore = Math.Min(100, researchFitScore + 10);
                }

                // 3. DYNAMIC WEIGHT SHIFTING
                double overallScore;
                if (isResearchTrack)
                {
                    overallScore = (basicAcademicScore * 0.35) + (greScore * 0.25) + (researchFitScore * 0.40);
                }
                else
                {
                    // Coursework Track: Shift 100% of the weight to Academics and Test Scores
                    overallScore = (basicAcademicScore * 0.60) + (greScore * 0.40);
                }

                double raChance = isResearchTrack ? program.HistoricalRAChance + (researchFitScore * 0.2) : 0;
                double taChance = program.HistoricalTAChance + (profile.WorkExperienceMonths > 12 ? 15 : 5);

                // 4. Dynamic Reasoning
                string category;
                string reason;

                if (overallScore >= 85)
                {
                    if (isResearchTrack && researchFitScore >= 80)
                    {
                        category = "🟢 Prime Research Fit";
                        reason = $"Excellent semantic match with your interest in [{profile.ResearchInterests}]. High probability of RA funding.";
                    }
                    else if (!isResearchTrack)
                    {
                        category = "🟢 Prime Academic Fit";
                        reason = "Excellent academic alignment for a professional coursework track. High probability of admission.";
                    }
                    else
                    {
                        category = "🟡 Target Program";
                        reason = "Strong academic alignment, but research fit is moderate based on faculty publications.";
                    }
                }
                else if (overallScore >= 70)
                {
                    category = "🟡 Target Program";
                    reason = "Solid academic alignment. Good target for your profile.";
                }
                else
                {
                    category = "🟠 Ambitious / Reach";
                    reason = "Your profile is below the typical academic standards for this program.";
                }

                // 5. Build Result 
                decimal estimatedTuition = program.TotalTuitionFee;
                decimal estimatedLiving = program.University.EstimatedLivingCost;
                decimal totalCost = estimatedTuition + estimatedLiving;

                string locationStr = !string.IsNullOrWhiteSpace(program.University.StateOrCity)
                    ? $"{program.University.StateOrCity}, {program.University.Country}"
                    : program.University.Country;

                results.Add(new UniversityRecommendationResult
                {
                    University = program.University,
                    Program = program,
                    AdmissionChance = Math.Round(basicAcademicScore, 1),
                    BudgetFit = Math.Round(researchFitScore, 1),
                    RaChance = Math.Round(Math.Min(100, raChance), 1),
                    TaChance = Math.Round(Math.Min(100, taChance), 1),
                    OverallScore = Math.Round(overallScore, 1),
                    Category = category,
                    RecommendationReason = reason,
                    TotalEstimatedCost = totalCost,
                    LocationDisplay = locationStr,
                    IsResearchTrack = isResearchTrack // Flag for the UI
                });
            } // END OF FOREACH LOOP

            return results.OrderByDescending(r => r.OverallScore).ToList();
        }

        private double CalculateResearchFit(string? studentInterests, string? programKeywords)
        {
            if (string.IsNullOrWhiteSpace(studentInterests) || string.IsNullOrWhiteSpace(programKeywords))
                return 30.0;

            var sWords = studentInterests.ToLower().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var pWords = programKeywords.ToLower().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var matchCount = sWords.Intersect(pWords).Count();

            if (matchCount == 0) return 40.0;

            double score = (double)matchCount / Math.Min(sWords.Length, pWords.Length) * 100.0;
            return Math.Min(100, score + 25);
        }
    }
}