namespace JobBoard.ApiService.Features.Vacancies.Models
{
    public class Vacancy
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // Логическая связь (кто создал)
        public string Title { get; set; } = string.Empty;
        public string DescriptionMarkdown { get; set; } = string.Empty;
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public bool IsArchived { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
