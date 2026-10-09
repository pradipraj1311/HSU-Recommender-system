namespace HSU_Recommender_system.Models
{
    public class ImportDTO
    {
        public string UniversityName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? StateOrCity { get; set; }
        public decimal EstimatedLivingCost { get; set; }
        public string? CarnegieClassification { get; set; }
        public double StudentFacultyRatio { get; set; }
        public int Ranking { get; set; }


public string DegreeName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public decimal TotalTuitionFee { get; set; }
        public double MinCGPA { get; set; }
        public double MinIELTS { get; set; }
        public int TargetGREQuant { get; set; }
        public bool IsGRERequired { get; set; }
        public int HistoricalRAChance { get; set; }
        public int HistoricalTAChance { get; set; }
        public string? ResearchKeywords { get; set; }
        public string? OpenAlexInstitutionId { get; set; }
    }
}