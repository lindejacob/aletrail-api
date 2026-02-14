using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using aletrail_api.Dtos.User;
using aletrail_api.Services.Auth;

namespace aletrail_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<UserController> _logger;

    public UserController(IAuthService authService, ILogger<UserController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
    {
        try
        {
            var userId = await _authService.Register(dto);
            if (userId == null)
            {
                _logger.LogError("Register returned null for user {Email}", dto.Email);
                return Problem("Could not create user", statusCode: 500);
            }

            // Return 201 Created with the new user's id in the body.
            return Created(string.Empty, new { id = userId.Value });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Registration failed for {Email}", dto.Email);
            // Map known conflict messages from the service to 409 Conflict
            if (ex.Message.Contains("Username already exists", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Email already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { error = ex.Message });
            }

            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        try
        {
            var token = await _authService.Login(dto);
            if (token == null) return Unauthorized(new { error = "Invalid credentials" });

            return Ok(new AuthResponseDto { Token = token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {Email}", dto.Email);
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }
}