using System;
using System.Collections.Generic;

namespace HRGroup.Models;

public partial class Applicant
{
    public string ApplicantId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Status { get; set; }

    public DateTime? AppliedDate { get; set; }

    public virtual ICollection<ApplicantVacancy> ApplicantVacancies { get; set; } = new List<ApplicantVacancy>();
}
