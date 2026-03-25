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

            // Identity Schema
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("users", "identity");
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.Email).IsUnique();
            });

            // Resume
            modelBuilder.Entity<Resume>(b =>
            {
                b.ToTable("resumes", "resumes");
                b.HasKey(x => x.Id);

                // Configure Skills as JSON and explicitly map its nested objects
                b.OwnsOne(x => x.Skills, sb =>
                {
                    sb.ToJson(); // Tells EF that the Skills object is JSON

                    // Explicitly define that Languages is a collection inside this JSON
                    sb.OwnsMany(s => s.Languages);
                });

                b.Property(x => x.ContactMethods).HasColumnType("jsonb");

                b.HasMany(x => x.WorkExperiences)
                    .WithOne(x => x.Resume)
                    .HasForeignKey(x => x.ResumeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // WorkExperience
            modelBuilder.Entity<WorkExperience>(b =>
            {
                b.ToTable("work_experiences", "resumes");
                b.HasKey(x => x.Id);
                b.Property(x => x.Technologies).HasColumnType("jsonb");
            });

            // Vacancy
            modelBuilder.Entity<Vacancy>(b =>
            {
                b.ToTable("vacancies", "vacancies");
                b.HasKey(x => x.Id);
                b.Property(x => x.SalaryFrom).HasColumnType("decimal(18,2)");
                b.Property(x => x.SalaryTo).HasColumnType("decimal(18,2)");
            });

            // Application
            modelBuilder.Entity<Application>(b =>
            {
                b.ToTable("applications", "responses");
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.VacancyId, x.ResumeId }).IsUnique();
            });
        }
    }
}