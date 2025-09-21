using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using App.Config;
using App.Data;
using App.Domain;
using App.DTOs;

namespace App.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly AppDbContext _context;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        AppDbContext context,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<(bool Success, string? Error, RegisterResponse? Response)> RegisterAsync(RegisterRequest request)
    {
        // Validate age (must be 18+)
        var age = DateTime.UtcNow.Year - request.BirthDate.Year;
        if (request.BirthDate.AddYears(age) > DateTime.UtcNow) age--;

        if (age < 18)
        {
            return (false, "You must be at least 18 years old to register", null);
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return (false, "Email already in use", null);
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors, null);
        }

        // Create profile
        var profile = new Profile
        {
            UserId = user.Id,
            DisplayName = request.DisplayName,
            BirthDate = DateTime.SpecifyKind(request.BirthDate, DateTimeKind.Utc),
            Gender = request.Gender
        };

        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync();

        return (true, null, new RegisterResponse { UserId = user.Id });
    }

    public async Task<(bool Success, string? Error, AuthResponse? Response)> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return (false, "Invalid credentials", null);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return (false, "Invalid credentials", null);
        }

        var (accessToken, accessExpiry) = GenerateAccessToken(user);
        var (refreshToken, refreshExpiry) = await GenerateRefreshTokenAsync(user);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            AccessExpiresAt = accessExpiry,
            RefreshToken = refreshToken,
            RefreshExpiresAt = refreshExpiry
        };

        return (true, null, response);
    }

    public async Task<(bool Success, string? Error, AuthResponse? Response)> RefreshAsync(RefreshRequest request)
    {
        var tokenEntity = _context.RefreshTokens
            .FirstOrDefault(rt => rt.Token == request.RefreshToken && rt.RevokedAt == null);

        if (tokenEntity == null || tokenEntity.ExpiresAt <= DateTime.UtcNow)
        {
            return (false, "Invalid or expired refresh token", null);
        }

        var user = await _userManager.FindByIdAsync(tokenEntity.UserId.ToString());
        if (user == null)
        {
            return (false, "User not found", null);
        }

        // Revoke old refresh token
        tokenEntity.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var (accessToken, accessExpiry) = GenerateAccessToken(user);
        var (refreshToken, refreshExpiry) = await GenerateRefreshTokenAsync(user);

        await _context.SaveChangesAsync();

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            AccessExpiresAt = accessExpiry,
            RefreshToken = refreshToken,
            RefreshExpiresAt = refreshExpiry
        };

        return (true, null, response);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var tokenEntity = _context.RefreshTokens
            .FirstOrDefault(rt => rt.Token == refreshToken && rt.RevokedAt == null);

        if (tokenEntity != null)
        {
            tokenEntity.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    private (string Token, DateTime Expiry) GenerateAccessToken(User user)
    {
        var expiry = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expiry);
    }

    private async Task<(string Token, DateTime Expiry)> GenerateRefreshTokenAsync(User user)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshDays);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = expiry
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return (token, expiry);
    }
}