using Microsoft.AspNetCore.Mvc;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

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
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();

            return Ok(users);
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