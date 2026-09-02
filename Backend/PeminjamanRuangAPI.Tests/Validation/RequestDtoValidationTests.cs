using System.ComponentModel.DataAnnotations;
using PeminjamanRuangAPI.DTOs;

namespace PeminjamanRuangAPI.Tests.Validation;

public class RequestDtoValidationTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            results,
            validateAllProperties: true);

        return results;
    }

    // =========================================================
    // BOOKING
    // =========================================================

    [Fact]
    public void CreateBooking_RoomIdZero_ShouldBeInvalid()
    {
        var request = CreateValidBooking();
        request.RoomId = 0;

        var results = Validate(request);

        Assert.Contains(
            results,
            x => x.MemberNames.Contains(nameof(request.RoomId)));
    }

    [Fact]
    public void CreateBooking_NumPeopleZero_ShouldBeInvalid()
    {
        var request = CreateValidBooking();
        request.NumPeople = 0;

        var results = Validate(request);

        Assert.Contains(
            results,
            x => x.MemberNames.Contains(nameof(request.NumPeople)));
    }

    [Fact]
    public void CreateBooking_RequesterNameTooLong_ShouldBeInvalid()
    {
        var request = CreateValidBooking();
        request.RequesterName = new string('A', 151);

        var results = Validate(request);

        Assert.Contains(
            results,
            x => x.MemberNames.Contains(nameof(request.RequesterName)));
    }

    [Fact]
    public void CreateBooking_ValidRequest_ShouldBeValid()
    {
        var request = CreateValidBooking();

        var results = Validate(request);

        Assert.Empty(results);
    }

    // =========================================================
    // MAINTENANCE
    // =========================================================

    [Fact]
    public void CreateMaintenance_RoomIdZero_ShouldBeInvalid()
    {
        var request = CreateValidMaintenance();
        request.RoomId = 0;

        var results = Validate(request);

        Assert.Contains(
            results,
            x => x.MemberNames.Contains(nameof(request.RoomId)));
    }

    [Fact]
    public void CreateMaintenance_DescriptionTooLong_ShouldBeInvalid()
    {
        var request = CreateValidMaintenance();
        request.Description = new string('A', 1001);

        var results = Validate(request);

        Assert.Contains(
            results,
            x => x.MemberNames.Contains(nameof(request.Description)));
    }

    [Fact]
    public void CreateMaintenance_ValidRequest_ShouldBeValid()
    {
        var request = CreateValidMaintenance();

        var results = Validate(request);

        Assert.Empty(results);
    }

    // =========================================================
    // CLEANING
    // =========================================================

    [Fact]
    public void Cleaning_CustomDurationZero_ShouldBeInvalid()
    {
        var request = new SetCleaningDurationRequestDto
        {
            CleaningDuration = "CUSTOM",
            CustomDurationMinutes = 0
        };

        var results = Validate(request);

        Assert.Contains(
            results,
            x => x.MemberNames.Contains(
                nameof(request.CustomDurationMinutes)));
    }

    [Fact]
    public void Cleaning_CustomDurationAbove1440_ShouldBeInvalid()
    {
        var request = new SetCleaningDurationRequestDto
        {
            CleaningDuration = "CUSTOM",
            CustomDurationMinutes = 1441
        };

        var results = Validate(request);

        Assert.Contains(
            results,
            x => x.MemberNames.Contains(
                nameof(request.CustomDurationMinutes)));
    }

    [Fact]
    public void Cleaning_CustomDurationWithinRange_ShouldBeValid()
    {
        var request = new SetCleaningDurationRequestDto
        {
            CleaningDuration = "CUSTOM",
            CustomDurationMinutes = 60
        };

        var results = Validate(request);

        Assert.Empty(results);
    }

    // =========================================================
    // LOGIN
    // =========================================================

    [Fact]
    public void Login_InvalidEmail_ShouldBeInvalid()
    {
        var request = new LoginRequestDto
        {
            Email = "bukan-email",
            Password = "password123"
        };

        var results = Validate(request);

        Assert.Contains(
            results,
            x => x.MemberNames.Contains(nameof(request.Email)));
    }

    [Fact]
    public void Login_ValidRequest_ShouldBeValid()
    {
        var request = new LoginRequestDto
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var results = Validate(request);

        Assert.Empty(results);
    }

    // =========================================================
    // REGISTER
    // =========================================================

    [Fact]
    public void Register_InvalidEmail_ShouldBeInvalid()
    {
        var request = CreateValidRegister();
        request.Email = "email-salah";

        var results = Validate(request);

        Assert.Contains(
            results,
            x => x.MemberNames.Contains(nameof(request.Email)));
    }

    [Fact]
    public void Register_PasswordBelowEightCharacters_ShouldBeInvalid()
    {
        var request = CreateValidRegister();
        request.Password = "1234567";

        var results = Validate(request);

        Assert.Contains(
            results,
            x => x.MemberNames.Contains(nameof(request.Password)));
    }

    [Fact]
    public void Register_DepartmentIdZero_ShouldBeInvalid()
    {
        var request = CreateValidRegister();
        request.DepartmentId = 0;

        var results = Validate(request);

        Assert.Contains(
            results,
            x => x.MemberNames.Contains(nameof(request.DepartmentId)));
    }

    [Fact]
    public void Register_ValidRequest_ShouldBeValid()
    {
        var request = CreateValidRegister();

        var results = Validate(request);

        Assert.Empty(results);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private static CreateBookingRequestDto CreateValidBooking()
    {
        return new CreateBookingRequestDto
        {
            RoomId = 1,
            BookingDate = DateOnly.FromDateTime(
                DateTime.Today.AddDays(1)),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            NumPeople = 5,
            Title = "Meeting",
            RequesterName = "Test User",
            RequesterDivision = "IT",
            Description = "Regression test"
        };
    }

    private static CreateMaintenanceRequestDto CreateValidMaintenance()
    {
        return new CreateMaintenanceRequestDto
        {
            RoomId = 1,
            MaintenanceCategory = "GENERAL",
            PriorityLevel = "MEDIUM",
            Description = "Maintenance test",
            StartDate = DateOnly.FromDateTime(
                DateTime.Today.AddDays(1)),
            EndDate = DateOnly.FromDateTime(
                DateTime.Today.AddDays(2))
        };
    }

    private static RegisterRequestDto CreateValidRegister()
    {
        return new RegisterRequestDto
        {
            Email = "newuser@example.com",
            Password = "password123",
            FullName = "Test User",
            PhoneNumber = "081234567890",
            DepartmentId = 1
        };
    }
}