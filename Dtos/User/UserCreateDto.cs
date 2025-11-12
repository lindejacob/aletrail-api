using System.ComponentModel.DataAnnotations;

namespace aletrail_api.Dtos.User;

public class UserCreateDto
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public required string Username { get; set; }
    
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(254, ErrorMessage = "Email cannot be longer than 254 characters.")]
    public required string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}