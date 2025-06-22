using System;
using System.Collections.Generic;

namespace HRGroup.Models;

public partial class ContactU
{
    public int Id { get; set; }

    public string? From { get; set; }

    public string? Email { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }
}
