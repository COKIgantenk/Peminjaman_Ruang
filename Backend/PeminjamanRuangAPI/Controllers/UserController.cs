using System.Security.Claims;
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
    [Authorize(Roles = "ADMIN")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordService _passwordService;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly UserTransactionService _userTransactionService;

        public UserController(
            IUserRepository userRepository,
            PasswordService passwordService,
            IDepartmentRepository departmentRepository,
            UserTransactionService userTransactionService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _departmentRepository = departmentRepository;
            _userTransactionService = userTransactionService;
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
        public async Task<ActionResult> CreateUser(
            [FromBody]CreateUserRequestDto request)
        {
            request.Email = request.Email.Trim().ToLowerInvariant();
            request.FullName = request.FullName.Trim();
            request.PhoneNumber = request.PhoneNumber.Trim();
            request.Role = request.Role.Trim().ToUpperInvariant();

            var exist = 
                await _userRepository.UserExistsAsync(request.Email);

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

            var allowedRoles = new[] { "USER", "ADMIN" };

            var role = request.Role;

            if (!allowedRoles.Contains(role))
            {
                return BadRequest(new 
                { 
                    message = "Role tidak valid. Role harus USER atau ADMIN." 
                });
            }

            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token Admin tidak valid"
                });
            }

            var passwordHash = _passwordService.HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                DepartmentId = request.DepartmentId,
                Role = request.Role,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var userId = await _userTransactionService.CreateUserAsync(
                user,
                adminId);
            
            if (userId <= 0)
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();

            var response = users.Select(user => new UserResponseDto
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
            });

            return Ok(response);
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(
            int id,
            [FromBody] UpdateUserRequestDto request)
        {
            request.FullName = request.FullName.Trim();
            request.PhoneNumber = request.PhoneNumber.Trim();
            request.Role = request.Role.Trim().ToUpperInvariant();

            var user = 
                await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new 
                { 
                    message = "User tidak ditemukan." 
                });
            }

            var department = await _departmentRepository
                .GetDepartmentByIdAsync(request.DepartmentId);

            if (department == null)
            {
                return BadRequest(new 
                { 
                    message = "Department tidak ditemukan." 
                });
            }

            var allowedRoles = new[] { "USER", "ADMIN" };
            var role = request.Role;

            if (!allowedRoles.Contains(role))
            {
                return BadRequest(new 
                { 
                    message = "Role tidak valid. Role harus USER atau ADMIN." 
                });
            }

            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token Admin tidak valid"
                });
            }

            if (adminId == id)
            {
                if (!request.IsActive)
                {
                    return BadRequest(new
                    {
                        message = "Admin tidak dapat menonaktifkan akun sendiri."
                    });
                }

                if (role != "ADMIN")
                {
                    return BadRequest(new
                    {
                        message = "Admin tidak dapat mengubah role akun sendiri."
                    });
                }
            }


            var oldFullName = user.FullName;
            var oldPhoneNumber = user.PhoneNumber;
            var oldDepartmentId = user.DepartmentId;
            var oldRole = user.Role;
            var oldIsActive = user.IsActive;

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.DepartmentId = request.DepartmentId;
            user.Role = role;
            user.IsActive = request.IsActive;
            
            var removesAdminAccess =
                oldRole == "ADMIN" &&
                oldIsActive &&
                (user.Role != "ADMIN" || !user.IsActive);
            
            if (removesAdminAccess)
            {
                var activeAdminCount =
                    await _userRepository.CountActiveAdminAsync();
            
                if (activeAdminCount <= 1)
                {
                    return BadRequest(new
                    {
                        message = "Tidak dapat menonaktifkan atau mengubah role admin terakhir."
                    });
                }
            }

            var success =
                await _userTransactionService.UpdateUserAsync(
                    user,
                    adminId,
                    oldFullName,
                    oldPhoneNumber,
                    oldDepartmentId,
                    oldRole,
                    oldIsActive);
            
            if (!success)
            {
                return BadRequest(new
                {
                    message = "Gagal memperbarui user."
                });
            }

            return Ok(new 
            { 
                message = "User berhasil diperbarui." 
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = 
                await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new 
                { 
                    message = "User tidak ditemukan." 
                });
            }

            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token admin tidak valid."
                });
            }

            if (adminId == id)
            {
                return BadRequest(new
                {
                    message = "Admin tidak dapat menghapus akun sendiri."
                });
            }

            if (user.Role == "ADMIN" && user.IsActive)
            {
                var activeAdminCount =
                    await _userRepository.CountActiveAdminAsync();

                if (activeAdminCount <= 1)
                {
                    return BadRequest(new
                    {
                        message = "Admin aktif terakhir tidak dapat dihapus."
                    });
                }
            }

            var success = await _userTransactionService.DeleteUserAsync(
                user,
                adminId);
            
            if (!success)
            {
                return BadRequest(new
                {
                    message = "Gagal menghapus user."
                });
            }

            return Ok(new 
            { 
                message = "User berhasil dihapus." 
            });
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult> RestoreUser(int id)
        {
            var deletedUser =
                await _userRepository.GetDeletedUserByIdAsync(id);
        
            if (deletedUser == null)
            {
                return NotFound(new
                {
                    message = "User yang sudah dihapus tidak ditemukan."
                });
            }
        
            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token admin tidak valid."
                });
            }
        
            var emailAlreadyUsed =
                await _userRepository.UserExistsAsync(deletedUser.Email);
        
            if (emailAlreadyUsed)
            {
                return Conflict(new
                {
                    message = "User tidak dapat dipulihkan karena email sudah digunakan oleh akun lain."
                });
            }
        
            var success = await _userTransactionService.RestoreUserAsync(
                deletedUser,
                adminId);
            
            if (!success)
            {
                return BadRequest(new
                {
                    message = "Gagal memulihkan user."
                });
            }
        
            return Ok(new
            {
                message = "User berhasil dipulihkan."
            });
        }
    }
}