namespace JobBoard.ApiService.Models
{
    public class Application
    {
        public Guid Id { get; set; }
        public Guid VacancyId { get; set; } // Логическая связь
        public Guid ResumeId { get; set; } // Логическая связь
        public DateTimeOffset AppliedAt { get; set; }
    }
}
