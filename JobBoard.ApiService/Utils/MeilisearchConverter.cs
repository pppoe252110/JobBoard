using JobBoard.ApiService.Features.Vacancies.Models;

namespace JobBoard.ApiService.Utils
{
    public static class MeilisearchConverter
    {
        public static dynamic ConvertResume(Resume resume)
        {
            var hardSkills = new List<string>();
            if (resume.Skills != null)
            {
                hardSkills.AddRange(resume.Skills.HardSkills);
                hardSkills.AddRange(resume.Skills.Languages.Select(s => s.Name));
            }
            foreach (var we in resume.WorkExperiences)
            {
                if (we.Technologies != null)
                    hardSkills.AddRange(we.Technologies);
            }
            var uniqueHardSkills = hardSkills.Distinct().ToList();

            var searchDocument = new
            {
                id = resume.Id,
                fullName = resume.FullName,
                title = resume.Title,
                location = resume.Location,
                expectedSalary = resume.ExpectedSalary,
                isVisible = resume.IsVisible,
                aboutMe = resume.AboutMe,
                experienceYears = resume.ExperienceYears,
                updatedAt = resume.UpdatedAt,
                skills = new { hardSkills = uniqueHardSkills }
            };

            return searchDocument;
        }

        public static dynamic ConvertVacancy(Vacancy vacancy)
        {
            var searchDocument = new
            {
                id = vacancy.Id,
                title = vacancy.Title,
                descriptionMarkdown = vacancy.DescriptionMarkdown,
                salaryFrom = vacancy.SalaryFrom,
                salaryTo = vacancy.SalaryTo,
                location = vacancy.Location,
                isRemote = vacancy.IsRemote,
                isArchived = false,
                createdAt = vacancy.CreatedAt
            };
            return searchDocument;
        }
    }
}
