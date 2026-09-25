namespace HSU_Recommender_system.Models
{
    public class UniversityRecommendationResult
    {
        public University? University { get; set; }
        public AcademicProgram? Program { get; set; }
        
        public double AdmissionChance { get; set; }
        public double BudgetFit { get; set; }
        public double RaChance { get; set; }
        public double TaChance { get; set; }
        public double OverallScore { get; set; }
        
        public string Category { get; set; } = string.Empty;
        public string RecommendationReason { get; set; } = string.Empty;
    }
}

