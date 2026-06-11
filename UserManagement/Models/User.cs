using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string Password { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public UserStatus UserStatus { get; set; }
}