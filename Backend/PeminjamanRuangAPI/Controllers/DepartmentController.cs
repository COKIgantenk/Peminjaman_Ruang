using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PeminjamanRuangAPI.Services;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly AuditLogService _auditLogService;

        public DepartmentController(
            IDepartmentRepository departmentRepository,
            AuditLogService auditLogService)
        {
            _departmentRepository = departmentRepository;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetAllDepartments()
        {
            var departments = await _departmentRepository.GetAllDepartmentsAsync();

            return Ok(departments);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> CreateDepartment(
            [FromBody] Department department)
        {
            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(adminIdClaim,out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token Admin tidak valid"
                });
            }

            var departmentId = 
                await _departmentRepository.CreateDepartmentAsync(department);

            if (departmentId <= 0)
            {
                return BadRequest(new
                {
                    message = "Department gagal dibuat"
                });
            }

            department.Id = departmentId;

            await _auditLogService.LogAsync(
                adminId,
                "CREATE",
                "DEPARTMENT",
                departmentId,
                $"Departemen '{department.Name}' dibuat.");

            return Ok(new
            {
                message = "Departemen berhasil dibuat.",
                id = departmentId
            });
        }
    }
}