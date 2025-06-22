using System;
using System.Collections.Generic;

namespace HRGroup.Models;

public partial class ApplicantVacancy
{
    public int Id { get; set; }

    public string ApplicantId { get; set; } = null!;

    public string VacancyId { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? AttachedDate { get; set; }

    public bool? IsInterviewScheduled { get; set; }

    public DateTime? InterviewScheduledDate { get; set; }

    public int? InterviewerId { get; set; }

    public virtual Applicant Applicant { get; set; } = null!;

    public virtual Employee? Interviewer { get; set; }

    public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();

    public virtual Vacancy Vacancy { get; set; } = null!;
}
