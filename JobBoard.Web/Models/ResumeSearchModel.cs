namespace JobBoard.Web.Models;

public class ResumeSearchModel
{
    public string? Query { get; set; }
    public int? MinExperience { get; set; }
    public string? RequiredSkill { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}