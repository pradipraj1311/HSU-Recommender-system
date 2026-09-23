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

        [Required]
        [Range(0.0, 10.0, ErrorMessage = "CGPA must be between 0 and 10")]
        public double CGPA { get; set; }

        [Required]
        [Display(Name = "IELTS / TOEFL Score")]
        public double EnglishProficiencyScore { get; set; }

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