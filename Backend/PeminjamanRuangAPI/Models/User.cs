namespace PeminjamanRuangAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string FullName { get; set; }
        public required string PhoneNumber { get; set; }

        public int DepartmentId { get; set; }

        public required string Role { get; set; } // USER, ADMIN

        public bool IsActive { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}