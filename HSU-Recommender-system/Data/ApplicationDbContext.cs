
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HSU_Recommender_system.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<University> Universities { get; set; }
        public DbSet<AcademicProgram> AcademicPrograms { get; set; }
        public DbSet<AdmissionDeadline> AdmissionDeadlines { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
    }
}


