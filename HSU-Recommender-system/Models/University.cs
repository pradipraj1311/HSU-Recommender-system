using HSU_Recommender_system    .Models;
using System.ComponentModel.DataAnnotations;

namespace HSU_Recommender_system.Models
{
    public class University
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        public string? StateOrCity { get; set; }

        [Display(Name = "Estimated Yearly Living Cost (USD)")]
        public decimal EstimatedLivingCost { get; set; }

        [Display(Name = "Global Ranking")]
        public int Ranking { get; set; }
        public ICollection<AcademicProgram>? Programs { get; set; }

    }
}
