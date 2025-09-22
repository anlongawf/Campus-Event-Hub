using System.ComponentModel.DataAnnotations;

namespace CampusEventHub.Models;

public class VerifyEmailModel
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Code { get; set; }
}