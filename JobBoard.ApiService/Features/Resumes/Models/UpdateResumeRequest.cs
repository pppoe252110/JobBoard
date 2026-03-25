namespace JobBoard.ApiService.Features.Resumes.Models;

public class UpdateResumeRequest : CreateResumeRequest
{
    public Guid Id { get; set; }
}