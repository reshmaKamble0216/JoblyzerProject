using System;
using System.Collections.Generic;

namespace JobLyzerProject.DTOs
{
    public record CreateJobDto(
        string Title,
        string Description,
        string? Location,
        List<string>? RequiredSkills
    );

    public record UpdateJobDto(
        string Title,
        string Description,
        string? Location,
        bool IsActive
    );

    public record UpdateApplicationStatusDto(
        string Status // Applied, Shortlisted, Exam Scheduled, Rejected
    );
}
