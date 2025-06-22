using System;
using System.Collections.Generic;

namespace HRGroup.Models;

public partial class Interview
{
    public int InterviewId { get; set; }

    public int ApplicantVacancyId { get; set; }

    public DateTime ScheduledDate { get; set; }

    public int InterviewerId { get; set; }

    public string? Result { get; set; }

    public string? Notes { get; set; }

    public virtual ApplicantVacancy ApplicantVacancy { get; set; } = null!;

    public virtual Employee Interviewer { get; set; } = null!;
}
