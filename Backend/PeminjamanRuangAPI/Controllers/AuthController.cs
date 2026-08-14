using Microsoft.AspNetCore.Mvc;
using PeminjamanRuangAPI.DTOs;
using PeminjamanRuangAPI.Repositories;
using PeminjamanRuangAPI.Services;

namespace PeminjamanRuangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordService _passwordService;
        private readonly JwtService _jwtService;

        public AuthController(
            IUserRepository userRepository,
            PasswordService passwordService,
            JwtService jwtService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(
            [FromBody] LoginRequestDto request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Email atau password salah."
                });
            }

            var passwordValid = _passwordService.VerifyPassword(
                request.Password,
                user.PasswordHash
            );

            if (!passwordValid)
            {
                return Unauthorized(new
                {
                    message = "Email atau password salah."
                });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new
                {
                    message = "User tidak aktif."
                });
            }

            var token = _jwtService.GenerateToken(user);

            var response = new LoginResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive,
                Token = token
            };

            return Ok(response);
        }
    }
}