using AirplaneSystem.Application.DTOs.Auth;
using AirplaneSystem.Application.DTOs.Users;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default);
    Task RevokeTokenAsync(string token, string ipAddress, CancellationToken ct = default);
    Task<bool> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);
    Task SendPasswordResetEmailAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}
