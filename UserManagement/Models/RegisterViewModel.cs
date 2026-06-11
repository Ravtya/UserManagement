using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models;

public class RegisterViewModel
{
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Incorrect email format")]
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}