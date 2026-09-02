using System.ComponentModel.DataAnnotations;

namespace PeminjamanRuangAPI.DTOs
{
    public class UpdateRoomRequestDto
    {
        [Required]
        [RegularExpression(@".*\S.*",
            ErrorMessage = "Name tidak boleh kosong atau hanya whitespace.")]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [RegularExpression(@".*\S.*",
            ErrorMessage = "Location tidak boleh kosong atau hanya whitespace.")]
        [StringLength(200)]
        public required string Location { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Capacity harus lebih dari 0.")]
        public int Capacity { get; set; }

        [Required]
        [RegularExpression(@".*\S.*",
            ErrorMessage = "Description tidak boleh kosong atau hanya whitespace.")]
        [StringLength(1000)]
        public required string Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
    }
}