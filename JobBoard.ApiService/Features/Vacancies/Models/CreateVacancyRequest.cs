namespace JobBoard.ApiService.Features.Vacancies.Models
{
    public class CreateVacancyRequest
    {
        public string Title { get; set; } = string.Empty;
        public string DescriptionMarkdown { get; set; } = string.Empty;
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
    }
}
