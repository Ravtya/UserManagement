namespace UserManagement.Models;

public class UserListItemViewModel
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserStatus UserStatus { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
}