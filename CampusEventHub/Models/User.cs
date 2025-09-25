using System.ComponentModel.DataAnnotations;

namespace CampusEventHub.Models;

public class User 
{
    [Key]
    public string UserId { get; set; } = Guid.NewGuid().ToString();
    public string? UserName { get; set; }
    [Required, EmailAddress]
    public string? Email { get; set; }
    public string? Password { get; set; }
    
    public int TrainningPoint { get; set; } = 0;
    
    public bool isAdmin { get; set; } = false;
    public bool IsVerified  { get; set; } = false;
    
    public DateTime RegistrationDate { get; set; } = DateTime.Now;

}