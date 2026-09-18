using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HSU_recommneder_system.Models
{
    public class AdmissionDeadline
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("AcademicProgram")]
        public int AcademicProgramId { get; set; }
        public AcademicProgram? AcademicProgram { get; set; }

        [Required]
        public string Term { get; set; } = string.Empty;

        public DateTime DeadlineDate { get; set; }
    }
}
