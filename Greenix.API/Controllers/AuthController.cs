using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Greenix.Shared.Models;
using Greenix.Application.Interfaces;
using Greenix.Application.DTOs.Auth;

namespace Greenix.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public AuthController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BaseResponse<AuthResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<AuthResponse>))]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<AuthResponse>("Invalid payload submitted."));
            }

            var response = await _identityService.RegisterCitizenAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return CreatedAtAction(nameof(Register), response);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<AuthResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(BaseResponse<AuthResponse>))]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<AuthResponse>("Invalid payload submitted."));
            }

            var response = await _identityService.LoginAsync(request);
            if (!response.Success)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<AuthResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<AuthResponse>))]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<AuthResponse>("Invalid payload submitted."));
            }

            var response = await _identityService.RefreshTokenAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<string>))]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>("Invalid payload submitted."));
            }

            var response = await _identityService.VerifyEmailAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<string>))]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>("Invalid payload submitted."));
            }

            var response = await _identityService.ForgotPasswordAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<string>))]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>("Invalid payload submitted."));
            }

            var response = await _identityService.ResetPasswordAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
