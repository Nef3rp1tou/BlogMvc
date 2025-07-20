using BlogMvc.DTOs;
using BlogMvc.DTOs.Auth;
using BlogMvc.DTOs.User;
using BlogMvc.Models.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogMvc.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
public class AuthController : BaseController
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    #region Authentication Endpoints

    [HttpPost]
    [ProducesResponseType(typeof(Result<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return CreateResponse(Result<LoginResponseDto>.Failure(
                Error.Validation("Invalid login request")));
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return CreateResponse(Result<LoginResponseDto>.Failure(
                Error.Unauthorized("Invalid email or password")));
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return CreateResponse(Result<LoginResponseDto>.Failure(
                Error.Unauthorized("Invalid email or password")));
        }

        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);

        var response = new LoginResponseDto
        {
            Token = token,
            Email = user.Email!,
            UserId = user.Id,
            Roles = roles.ToList(),
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        return CreateResponse(Result<LoginResponseDto>.Success(response));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<RegisterResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return CreateResponse(Result<RegisterResponseDto>.Failure(
                Error.Validation("Invalid registration request")));
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return CreateResponse(Result<RegisterResponseDto>.Failure(
                Error.Validation("Email is already registered")));
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true // For demo purposes
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return CreateResponse(Result<RegisterResponseDto>.Failure(
                Error.Validation(errors)));
        }

        // Add default role
        await _userManager.AddToRoleAsync(user, "User");

        var response = new RegisterResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Message = "Registration successful. You can now login."
        };

        return CreateResponse(Result<RegisterResponseDto>.Success(response));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Explicitly use JWT
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        // For JWT, logout is handled client-side by removing the token
        // This endpoint can be used for any server-side cleanup if needed

        return CreateResponse(Result.Success());
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] 
    [ProducesResponseType(typeof(Result<UserInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return CreateResponse(Result<UserInfoDto>.Failure(
                Error.Unauthorized("User not found")));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return CreateResponse(Result<UserInfoDto>.Failure(
                Error.Unauthorized("User not found")));
        }

        var roles = await _userManager.GetRolesAsync(user);

        var userInfo = new UserInfoDto
        {
            UserId = user.Id,
            Email = user.Email!,
            Roles = roles.ToList()
        };

        return CreateResponse(Result<UserInfoDto>.Success(userInfo));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] 
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return CreateResponse(Result.Failure(Error.Unauthorized("User not found")));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return CreateResponse(Result.Failure(Error.Unauthorized("User not found")));
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return CreateResponse(Result.Failure(Error.Validation(errors)));
        }

        return CreateResponse(Result.Success());
    }

    #endregion

    #region Private Methods

    private async Task<string> GenerateJwtToken(IdentityUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Sub, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(24);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    #endregion
}