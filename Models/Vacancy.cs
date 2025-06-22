using System;
using System.Collections.Generic;

namespace HRGroup.Models;

public partial class Vacancy
{
    public string VacancyId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string DepartmentId { get; set; } = null!;

    public string? JobDescription { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int Openings { get; set; }

    public string? Status { get; set; }

    public DateTime? ClosingDate { get; set; }

    public virtual ICollection<ApplicantVacancy> ApplicantVacancies { get; set; } = new List<ApplicantVacancy>();

    public virtual Employee CreatedByNavigation { get; set; } = null!;

    public virtual Department Department { get; set; } = null!;
}
