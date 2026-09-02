using Microsoft.AspNetCore.Mvc;
using PeminjamanRuangAPI.DTOs;
using PeminjamanRuangAPI.Repositories;
using PeminjamanRuangAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordService _passwordService;
        private readonly JwtService _jwtService;
        private readonly IDepartmentRepository _departmentRepository;

        public AuthController(
            IUserRepository userRepository,
            PasswordService passwordService,
            JwtService jwtService,
            IDepartmentRepository departmentRepository)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _departmentRepository = departmentRepository;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<LoginResponseDto>> Login(
            [FromBody] LoginRequestDto request)
        {
            request.Email = request.Email.Trim().ToLowerInvariant();
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

        [AllowAnonymous]
        [HttpPost("register")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult> Register(
            [FromBody] RegisterRequestDto request)
        {
            request.Email = request.Email.Trim().ToLowerInvariant();
            request.FullName = request.FullName.Trim();
            request.PhoneNumber = request.PhoneNumber.Trim();

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new
                {
                    message = "Nama lengkap harus diisi."
                });
            }
            
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest(new
                {
                    message = "Nomor telepon harus diisi."
                });
            }

            var exist = await _userRepository.UserExistsAsync(request.Email);

            if (exist)
            {
                return Conflict(new
                {
                    message = "User dengan email tersebut sudah ada."
                });
            }

            var department =
                await _departmentRepository
                    .GetDepartmentByIdAsync(request.DepartmentId);
            
            if (department == null)
            {
                return BadRequest(new
                {
                    message = "Department tidak ditemukan."
                });
            }

            var passwordHash = _passwordService.HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                DepartmentId = request.DepartmentId,

                //Register publik selalu USER
                Role = "USER",

                IsActive = true, // Set default status to active
            };

            var userId = await _userRepository.CreateUserAsync(user);

            if (userId <= 0)
            {
                return BadRequest(new
                {
                    message = "Registrasi gagal. Silakan coba lagi."
                });
            }

            return Ok(new
            {
                message = "Registrasi berhasil."
            });
        }
    }
}