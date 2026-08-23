namespace PeminjamanRuangAPI.DTOs
{
    public class SetCleaningDurationRequestDto
    {
        public required string CleaningDuration { get; set; }

        public int? CustomDurationMinutes { get; set; }
    }
}