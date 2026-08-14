namespace PeminjamanRuangAPI.DTOs
{
    public class LoginResponseDto
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}