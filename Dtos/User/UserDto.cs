using System.ComponentModel.DataAnnotations;

namespace aletrail_api.Dtos.User;

public class UserDto
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public required string Username { get; set; }
    
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(254, ErrorMessage = "Email cannot be longer than 254 characters.")]
    public required string Email { get; set; }
    
}