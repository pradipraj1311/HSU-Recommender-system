using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HSU_Recommender_system.Models
{
    public class ShortlistedProgram
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("AcademicProgram")]
        public int AcademicProgramId { get; set; }
        public AcademicProgram? AcademicProgram { get; set; }

        [Display(Name = "Application Status")]
        public string Status { get; set; } = "Saved"; 

        [Display(Name = "Date Added")]
        public DateTime AddedOn { get; set; } = DateTime.Now;
    }
}