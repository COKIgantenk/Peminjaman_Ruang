using Microsoft.AspNetCore.Mvc;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentController(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetAllDepartments()
        {
            var departments = await _departmentRepository.GetAllDepartmentsAsync();

            return Ok(departments);
        }

        [HttpPost]
        public async Task<ActionResult<Department>> CreateDepartment(Department department)
        {
            var createdDepartment = await _departmentRepository.CreateDepartmentAsync(department);

            return Ok(createdDepartment);
        }
    }
}