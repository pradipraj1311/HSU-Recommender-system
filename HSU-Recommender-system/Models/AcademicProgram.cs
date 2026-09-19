using HSU_Recommender_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HSU_Recommender_system.Models;
using HSU_Recommender_system.Data;


namespace HSU_Recommender_system.Models
{
    public class AcademicProgram
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("University")]
        public int UniversityId { get; set; }
        public University? University { get; set; }

        [Required]
        public string DegreeName { get; set; } = string.Empty; 

        public string? Department { get; set; }

        [Display(Name = "Total Tuition Fee (USD)")]
        public decimal TotalTuitionFee { get; set; }

        public double MinCGPA { get; set; }
        public double MinIELTS { get; set; }
    
        [Display(Name = "Historical RA Chance (%)")]
        public int HistoricalRAChance { get; set; }

        [Display(Name = "Historical TA Chance (%)")]
        public int HistoricalTAChance { get; set; }

        [Display(Name = "Research Fit Keywords (Comma separated)")]
        public string? ResearchKeywords { get; set; }

        public ICollection<AdmissionDeadline>? Deadlines { get; set; }
    }
}

