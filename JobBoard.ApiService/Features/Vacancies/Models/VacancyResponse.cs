namespace JobBoard.ApiService.Features.Vacancies.Models
{
    public class VacancyResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DescriptionMarkdown { get; set; } = string.Empty;
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsArchived { get; set; }
    }
}
