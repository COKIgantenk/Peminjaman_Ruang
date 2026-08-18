namespace PeminjamanRuangAPI.DTOs
{
    public class RegisterRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FullName { get; set; }
        public required string PhoneNumber { get; set; }
        public int DepartmentId { get; set; }
    }
}