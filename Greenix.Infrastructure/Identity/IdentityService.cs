using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Greenix.Shared.Models;
using Greenix.Shared.Constants;
using Greenix.Domain.Entities;
using Greenix.Application.Interfaces;
using Greenix.Application.DTOs.Auth;

namespace Greenix.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public IdentityService(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<BaseResponse<AuthResponse>> RegisterCitizenAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new BaseResponse<AuthResponse>("An account with this email address already exists.");
            }

            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return new BaseResponse<AuthResponse>(errors, "User registration failed.");
            }

            // Ensure the Citizen role exists
            if (!await _roleManager.RoleExistsAsync(Roles.Citizen))
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.Citizen));
            }

            // Assign Citizen role
            await _userManager.AddToRoleAsync(user, Roles.Citizen);

            // Generate Email verification token
            var verifyToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // Dispatch verification email
            await _emailService.SendEmailAsync(
                user.Email,
                "Welcome to Greenix - Verify your Email",
                $"Hello {user.FullName},\n\nThank you for signing up to Greenix! Please confirm your registration by providing the following token in the application:\n\nVerification Token: {verifyToken}\n\nTogether for a cleaner, greener city."
            );

            // Generate tokens
            var role = Roles.Citizen;
            var accessToken = _tokenService.GenerateAccessToken(user, role);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var authResponse = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email,
                FullName = user.FullName,
                Role = role,
                IsVerified = false
            };

            return new BaseResponse<AuthResponse>(authResponse, "Citizen registered successfully. Verification email sent.");
        }

        public async Task<BaseResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new BaseResponse<AuthResponse>("Invalid email or password.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return new BaseResponse<AuthResponse>("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                return new BaseResponse<AuthResponse>("This account has been suspended. Please contact the administrator.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? Roles.Citizen;

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user, role);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var authResponse = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = role,
                IsVerified = user.EmailConfirmed
            };

            return new BaseResponse<AuthResponse>(authResponse, "Login successful.");
        }

        public async Task<BaseResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                return new BaseResponse<AuthResponse>("Invalid access token or signature.");
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return new BaseResponse<AuthResponse>("Invalid access token claims.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new BaseResponse<AuthResponse>("Invalid or expired refresh token.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? Roles.Citizen;

            // Generate new token pair
            var newAccessToken = _tokenService.GenerateAccessToken(user, role);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var authResponse = new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = role,
                IsVerified = user.EmailConfirmed
            };

            return new BaseResponse<AuthResponse>(authResponse, "Token refreshed successfully.");
        }

        public async Task<BaseResponse<string>> VerifyEmailAsync(VerifyEmailRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return new BaseResponse<string>("User not found.");
            }

            var confirmResult = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!confirmResult.Succeeded)
            {
                var errors = confirmResult.Errors.Select(e => e.Description).ToList();
                return new BaseResponse<string>(errors, "Email verification failed.");
            }

            return new BaseResponse<string>("Email verified successfully.", "Verification Success");
        }

        public async Task<BaseResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            // In a security-hardened environment, we return success even if user is not found to prevent account enumeration.
            if (user == null)
            {
                return new BaseResponse<string>("If the email is registered, a password reset token has been sent.", "Request Processed");
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _emailService.SendEmailAsync(
                user.Email!,
                "Greenix - Reset Password Request",
                $"Hello {user.FullName},\n\nYou requested a password reset. Please use the following token to reset your password in the application:\n\nPassword Reset Token: {resetToken}\n\nIf you did not initiate this request, you can safely ignore this email."
            );

            return new BaseResponse<string>("If the email is registered, a password reset token has been sent.", "Request Processed");
        }

        public async Task<BaseResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new BaseResponse<string>("User not found.");
            }

            var resetResult = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!resetResult.Succeeded)
            {
                var errors = resetResult.Errors.Select(e => e.Description).ToList();
                return new BaseResponse<string>(errors, "Password reset failed.");
            }

            return new BaseResponse<string>("Password reset successfully. You can now log in.", "Reset Success");
        }
    }
}
