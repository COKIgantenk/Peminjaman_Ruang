using Microsoft.AspNetCore.Mvc;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;
using PeminjamanRuangAPI.DTOs;

namespace PeminjamanRuangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
        
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody]User user)
        {
            var exist = await _userRepository.UserExistsAsync(user.Email);

            if (exist)
            {
                return Conflict(new 
                { 
                    message = "User dengan email tersebut sudah ada." 
                });
            }

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
    }
}