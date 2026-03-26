namespace JobBoard.ApiService.Models
{
    public class Application
    {
        public Guid Id { get; set; }
        public Guid VacancyId { get; set; }
        public Guid ResumeId { get; set; }
        public DateTimeOffset AppliedAt { get; set; }

        public string Status { get; set; } = "Applied";
    }
}
