using System.ComponentModel.DataAnnotations;

namespace PeminjamanRuangAPI.DTOs
{
    public class CreateDepartmentRequestDto
    {
        [Required]
        [RegularExpression(
            @".*\S.*",
            ErrorMessage = "Name tidak boleh kosong atau hanya whitespace.")]
        [StringLength(100)]
        public required string Name { get; set; }
    }
}