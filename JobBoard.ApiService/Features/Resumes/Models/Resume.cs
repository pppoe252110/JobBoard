namespace JobBoard.ApiService.Features.Resumes.Models
{
    public class Resume
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // Логическая связь, без ForeignKey
        public string Title { get; set; } = string.Empty;
        public string[] Skills { get; set; } = Array.Empty<string>();
        public bool IsVisible { get; set; }
        public string AboutMe { get; set; } = string.Empty;

        // Навигационное свойство внутри одного модуля
        public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
    }
}
