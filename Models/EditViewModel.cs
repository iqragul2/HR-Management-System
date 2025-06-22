using System.ComponentModel.DataAnnotations;

namespace HRGroup.Models
{

public class EditViewModel
{
    public int UserId { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }
}
}
