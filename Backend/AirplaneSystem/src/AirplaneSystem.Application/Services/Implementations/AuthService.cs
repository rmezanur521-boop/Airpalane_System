using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Auth;
using AirplaneSystem.Application.DTOs.Users;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Users;
using AirplaneSystem.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AirplaneSystem.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;
    private readonly INotificationService _notification;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUnitOfWork uow, IMapper mapper, IConfiguration config,
        ILogger<AuthService> logger, INotificationService notification, IPasswordHasher passwordHasher)
    {
        _uow = uow;
        _mapper = mapper;
        _config = config;
        _logger = logger;
        _notification = notification;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _uow.Users.EmailExistsAsync(request.Email, ct))
            throw new ConflictException($"An account with email '{request.Email}' already exists.");

        var user = _mapper.Map<User>(request);
        user.PasswordHash = _passwordHasher.Hash(request.Password);
        user.EmailVerificationToken = GenerateSecureToken();
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        try { await _notification.SendEmailVerificationAsync(user.Id, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to send verification email for user {UserId}", user.Id); }

        _logger.LogInformation("User registered: {Email}", user.Email);

        var (accessToken, refreshToken) = await GenerateTokensAsync(user, "system", ct);
        return BuildAuthResponse(accessToken, refreshToken, user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email.ToLowerInvariant(), ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive) throw new UnauthorizedAccessException("Your account has been deactivated.");
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var (accessToken, refreshToken) = await GenerateTokensAsync(user, ipAddress, ct);
        _logger.LogInformation("User logged in: {Email} from {IP}", user.Email, ipAddress);
        return BuildAuthResponse(accessToken, refreshToken, user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default)
    {
        var hashedToken = HashToken(refreshToken);
        var user = await _uow.Users.GetByRefreshTokenAsync(hashedToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        var existingToken = user.RefreshTokens.First(t => t.Token == hashedToken);
        if (!existingToken.IsActive)
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");

        existingToken.IsRevoked = true;
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedByIp = ipAddress;

        var (newAccessToken, newRefreshToken) = await GenerateTokensAsync(user, ipAddress, ct);
        existingToken.ReplacedByToken = HashToken(newRefreshToken);

       // _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return BuildAuthResponse(newAccessToken, newRefreshToken, user);
    }

    public async Task RevokeTokenAsync(string token, string ipAddress, CancellationToken ct = default)
    {
        var hashedToken = HashToken(token);
        var user = await _uow.Users.GetByRefreshTokenAsync(hashedToken, ct)
            ?? throw new NotFoundException("Refresh token not found.");

        var rt = user.RefreshTokens.First(t => t.Token == hashedToken);
        if (!rt.IsActive) throw new UnauthorizedAccessException("Token already revoked.");

        rt.IsRevoked = true;
        rt.RevokedAt = DateTime.UtcNow;
        rt.RevokedByIp = ipAddress;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email, ct)
            ?? throw new NotFoundException("User", request.Email);

        if (user.IsEmailVerified) return true;
        if (user.EmailVerificationToken != request.Token || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            throw new ValidationException("token", "Email verification token is invalid or expired.");

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task SendPasswordResetEmailAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email, ct);
        if (user == null) return; // Don't reveal if email exists

        user.PasswordResetToken = GenerateSecureToken();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(2);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        await _notification.SendPasswordResetAsync(user.Id, user.PasswordResetToken, ct);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email, ct)
            ?? throw new NotFoundException("User", request.Email);

        if (user.PasswordResetToken != request.Token || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            throw new ValidationException("token", "Password reset token is invalid or expired.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);
        return _mapper.Map<UserDto>(user);
    }

    private async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user, string ipAddress, CancellationToken ct)
    {
        var accessToken = GenerateJwtToken(user);
        var refreshTokenValue = GenerateRefreshTokenValue();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = HashToken(refreshTokenValue),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress
        };
        //await _uow.Users.GetByIdAsync(user.Id, ct); // ensure tracked
        //user.RefreshTokens.Add(refreshToken);
        //_uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return (accessToken, refreshTokenValue);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSecret = _config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshTokenValue()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static AuthResponse BuildAuthResponse(string accessToken, string refreshToken, User user) =>
        new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 1800,
            User = new UserInfo
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Role = user.Role.ToString(),
                IsEmailVerified = user.IsEmailVerified
            }
        };
}
