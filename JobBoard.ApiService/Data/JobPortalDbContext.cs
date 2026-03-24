using JobBoard.ApiService.Features.Identity.Models;
using JobBoard.ApiService.Features.Resumes.Models;
using JobBoard.ApiService.Features.Vacancies.Models;
using JobBoard.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Data
{
    public class JobPortalDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Resume> Resumes => Set<Resume>();
        public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
        public DbSet<Vacancy> Vacancies => Set<Vacancy>();
        public DbSet<Application> Applications => Set<Application>();

        public JobPortalDbContext(DbContextOptions<JobPortalDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Identity Schema (identity.users)
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("users", "identity");
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.Email).IsUnique();
            });

            // Resumes Schema (resumes.resumes)
            modelBuilder.Entity<Resume>(b =>
            {
                b.ToTable("resumes", "resumes");
                b.HasKey(x => x.Id);

                // Specifically for PostgreSQL JSONB support
                b.Property(x => x.Skills).HasColumnType("jsonb");

                b.HasMany(x => x.WorkExperiences)
                 .WithOne(x => x.Resume)
                 .HasForeignKey(x => x.ResumeId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // WorkExperiences Schema (resumes.work_experiences)
            modelBuilder.Entity<WorkExperience>(b =>
            {
                // Note: Standardized to "work_experiences" using snake_case (Postgres convention)
                b.ToTable("work_experiences", "resumes");
                b.HasKey(x => x.Id);
            });

            // Vacancies Schema (vacancies.vacancies)
            modelBuilder.Entity<Vacancy>(b =>
            {
                b.ToTable("vacancies", "vacancies");
                b.HasKey(x => x.Id);
                b.Property(x => x.SalaryFrom).HasColumnType("decimal(18,2)");
                b.Property(x => x.SalaryTo).HasColumnType("decimal(18,2)");
            });

            // Applications Schema (responses.applications)
            modelBuilder.Entity<Application>(b =>
            {
                b.ToTable("applications", "responses");
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.VacancyId, x.ResumeId }).IsUnique();
            });
        }
    }
}