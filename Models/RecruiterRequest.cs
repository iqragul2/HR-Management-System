using System;
using System.Collections.Generic;

namespace HRGroup.Models;

public partial class RecruiterRequest
{
    public int RequestId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string ContactInfo { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public DateTime? RequestDate { get; set; }

    public string? DepartmentId { get; set; }

    public string? Status { get; set; }

    public virtual Department? Department { get; set; }

    public virtual User User { get; set; } = null!;
}
