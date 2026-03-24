namespace JobBoard.ApiService.Features.Resumes.Models
{
    public class WorkExperience
    {
        public Guid Id { get; set; }
        public Guid ResumeId { get; set; }
        public Resume Resume { get; set; } = null!; // Физическая связь с Resume
        public string CompanyName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; } // null = "по настоящее время"
        public string Description { get; set; } = string.Empty;
    }
}
