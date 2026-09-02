using System.ComponentModel.DataAnnotations;

namespace PeminjamanRuangAPI.DTOs
{
    public class CreateFacilityRequestDto
    {
        [Required]
        [RegularExpression(@".*\S.*",
            ErrorMessage = "Name tidak boleh kosong atau hanya whitespace.")]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [RegularExpression(@".*\S.*",
            ErrorMessage = "Description tidak boleh kosong atau hanya whitespace.")]
        [StringLength(1000)]
        public required string Description { get; set; }
    }
}