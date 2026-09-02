using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PeminjamanRuangAPI.Services;
using PeminjamanRuangAPI.DTOs;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FacilityController : ControllerBase
    {
        private readonly IFacilityRepository _facilityRepository;
        private readonly FacilityTransactionService _facilityTransactionService;

        public FacilityController(
            IFacilityRepository facilityRepository,
            FacilityTransactionService facilityTransactionService)
        {
            _facilityRepository = facilityRepository;
            _facilityTransactionService = facilityTransactionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacilityResponseDto>>> GetAllFacilities()
        {
            var facilities = await _facilityRepository.GetAllFacilitiesAsync();

            var response = facilities.Select(facility => new FacilityResponseDto
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                CreatedAt = facility.CreatedAt,
                UpdatedAt = facility.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FacilityResponseDto>> GetFacility(int id)
        {
            var facility = await _facilityRepository.GetFacilityByIdAsync(id);

            if (facility == null)
            {
                return NotFound(new
                {
                    message = "Facility tidak ditemukan."
                });
            }

            var response = new FacilityResponseDto
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                CreatedAt = facility.CreatedAt,
                UpdatedAt = facility.UpdatedAt
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> CreateFacility(
            [FromBody] CreateFacilityRequestDto request)
        {
            var facility = new Facility
            {
                Name = request.Name.Trim(),
                Description = request.Description.Trim()
            };

            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token Admin tidak valid"
                });
            }

            var facilityId =
                await _facilityTransactionService.CreateFacilityAsync(
                    facility,
                    adminId);
            
            if (facilityId <= 0)
            {
                return BadRequest(new
                {
                    message = "Facility gagal dibuat"
                });
            }

            return Ok(new
            {
                message = "Facility berhasil dibuat."
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> UpdateFacility(
            int id,
            [FromBody] UpdateFacilityRequestDto request)
        {
            var facility = await _facilityRepository.GetFacilityByIdAsync(id);

            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token Admin tidak valid"
                });
            }

            if (facility == null)
            {
                return NotFound(new
                {
                    message = "Facility tidak ditemukan."
                });
            }

            var oldName = facility.Name;
            var oldDescription = facility.Description;

            facility.Name = request.Name.Trim();
            facility.Description = request.Description.Trim();
            var success =
                await _facilityTransactionService.UpdateFacilityAsync(
                    facility,
                    adminId,
                    oldName,
                    oldDescription);
            
            if (!success)
            {
                return BadRequest(new
                {
                    message = "Facility gagal diperbarui."
                });
            }

            return Ok(new
            {
                message = "Facility berhasil diperbarui."
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> DeleteFacility(int id)
        {
            var facility = await _facilityRepository.GetFacilityByIdAsync(id);

            if (facility == null)
            {
                return NotFound(new
                {
                    message = "Facility tidak ditemukan."
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

            var success =
                await _facilityTransactionService.DeleteFacilityAsync(
                    facility,
                    adminId);
            
            if (!success)
            {
                return BadRequest(new
                {
                    message = "Facility gagal dihapus."
                });
            }

            return Ok(new
            {
                message = "Facility berhasil dihapus."
            });
        }
    }
}