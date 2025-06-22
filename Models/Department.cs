using System;
using System.Collections.Generic;

namespace HRGroup.Models;

public partial class Department
{
    public string DepartmentId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<RecruiterRequest> RecruiterRequests { get; set; } = new List<RecruiterRequest>();

    public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
}
