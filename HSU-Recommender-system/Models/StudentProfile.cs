using HSU_Recommender_system.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HSU_Recommender_system.Models
{
    public class StudentProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? ApplicationUser { get; set; }

        [Display(Name = "CGPA (Out of 10)")]
        [Range(0.0, 10.0, ErrorMessage = "CGPA must be measured on a 10-point scale (e.g., 7.5 or 8.0).")]
        public double CGPA { get; set; }

        [Display(Name = "IELTS / TOEFL Score")]
        [Range(0.0, 120.0, ErrorMessage = "Please enter a valid IELTS (0-9) or TOEFL (0-120) score.")]
        public double EnglishProficiencyScore { get; set; }

        [Display(Name = "GRE Quant Score")]
        [Range(130, 170, ErrorMessage = "The GRE Quantitative section is strictly scored between 130 and 170.")]
        public int GREQuantScore { get; set; }

        [Display(Name = "Core Research Interests (Comma separated)")]
        public string? ResearchInterests { get; set; }



        [Required]
        [Display(Name = "Maximum Budget (USD)")]
        public decimal MaximumBudget { get; set; }

        [Display(Name = "Number of Major Projects")]
        public int NumberOfProjects { get; set; }

        [Display(Name = "Number of Research Papers/Publications")]
        public int NumberOfResearchPapers { get; set; }

        [Display(Name = "Months of Work/Internship Experience")]
        public int WorkExperienceMonths { get; set; }

        [Display(Name = "Target Degree (e.g., MS in Computer Science)")]
        public string? TargetDegree { get; set; }

        [Display(Name = "Preferred Study Country")]
        public string? PreferredCountry { get; set; }
    }
}