using Microsoft.AspNetCore.Mvc;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;
using PeminjamanRuangAPI.DTOs;
using PeminjamanRuangAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace PeminjamanRuangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordService _passwordService;

        public UserController(
            IUserRepository userRepository,
            PasswordService passwordService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new 
                { 
                    message = "User tidak ditemukan." 
                });
            }

            var response = new UserResponseDto
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        PhoneNumber = user.PhoneNumber,
        DepartmentId = user.DepartmentId,
        Role = user.Role,
        IsActive = user.IsActive,
        LastLogin = user.LastLogin,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };

    return Ok(response);
}
        
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody]CreateUserRequestDto request)
        {
            var exist = await _userRepository.UserExistsAsync(request.Email);

            if (exist)
            {
                return Conflict(new 
                { 
                    message = "User dengan email tersebut sudah ada." 
                });
            }
            var passwordHash = _passwordService.HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash, // In a real application, you should hash the password before storing it
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                DepartmentId = request.DepartmentId,
                Role = request.Role,
                IsActive = true,
            };

            var success = await _userRepository.CreateUserAsync(user);

            if (!success)
            {
                return BadRequest(new 
                { 
                    message = "User gagal dibuat." 
                });
            }

            return Ok(new 
            { 
                message = "User berhasil dibuat." 
            });

            

        }

        [HttpGet("admin-test")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult AdminTest()
        {
            return Ok(new { message = "Akses berhasil. Anda adalah ADMIN." });
        }

        
    }
}