namespace JobBoard.ApiService.Features.Vacancies.Models
{
    public class UpdateVacancyRequest : CreateVacancyRequest
    {
        public Guid Id { get; set; }
    }
}
