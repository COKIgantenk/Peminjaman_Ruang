namespace PeminjamanRuangAPI.DTOs
{
    public class UpdateUserRequestDto
    {
        public required string FullName { get; set; }

        public required string PhoneNumber { get; set; }

        public int DepartmentId { get; set; }

        public required string Role { get; set; }

        public bool IsActive { get; set; }
    }
}