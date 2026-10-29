namespace HSU_Recommender_system.Models
{
    public class UniversityCsvDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? StateOrCity { get; set; }
        public decimal EstimatedLivingCost { get; set; }
        public int Ranking { get; set; }
        public string? CarnegieClassification { get; set; }
        public double StudentFacultyRatio { get; set; }
    }
}