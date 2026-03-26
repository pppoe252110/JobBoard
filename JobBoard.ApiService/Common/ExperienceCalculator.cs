using JobBoard.ApiService.Features.Resumes.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobBoard.ApiService.Common;

public static class ExperienceCalculator
{
    public static int CalculateTotalYears(IEnumerable<WorkExperience> experiences)
    {
        if (experiences == null || !experiences.Any())
            return 0;

        long totalDays = 0;
        var today = DateOnly.FromDateTime(DateTime.Today);

        foreach (var exp in experiences)
        {
            var start = exp.StartDate;
            var end = exp.EndDate ?? today;
            var days = end.DayNumber - start.DayNumber;
            totalDays += days;
        }

        // Convert days to years (approx 365.25 days per year)
        return (int)Math.Round(totalDays / 365.25);
    }
}