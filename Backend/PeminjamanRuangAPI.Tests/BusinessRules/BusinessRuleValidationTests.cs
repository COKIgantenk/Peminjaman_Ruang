using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PeminjamanRuangAPI.Controllers;
using PeminjamanRuangAPI.DTOs;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Tests.BusinessRules;

public class BusinessRuleValidationTests
{
    // =========================================================
    // BOOKING
    // =========================================================

    [Fact]
    public async Task Booking_StartTimeEqualsEndTime_Returns400()
    {
        var roomRepository =
            new Mock<IRoomRepository>();

        roomRepository
            .Setup(x => x.GetRoomByIdAsync(1))
            .ReturnsAsync(CreateActiveRoom());

        var controller =
            CreateBookingController(
                roomRepository.Object);

        SetAuthenticatedUser(
            controller,
            userId: 1001,
            role: "USER");

        var request =
            CreateValidBookingRequest();

        request.StartTime =
            new TimeOnly(10, 0);

        request.EndTime =
            new TimeOnly(10, 0);

        var result =
            await controller.CreateBooking(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Booking_StartTimeAfterEndTime_Returns400()
    {
        var roomRepository =
            new Mock<IRoomRepository>();

        roomRepository
            .Setup(x => x.GetRoomByIdAsync(1))
            .ReturnsAsync(CreateActiveRoom());

        var controller =
            CreateBookingController(
                roomRepository.Object);

        SetAuthenticatedUser(
            controller,
            userId: 1001,
            role: "USER");

        var request =
            CreateValidBookingRequest();

        request.StartTime =
            new TimeOnly(11, 0);

        request.EndTime =
            new TimeOnly(10, 0);

        var result =
            await controller.CreateBooking(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // =========================================================
    // MAINTENANCE
    // =========================================================

    [Fact]
    public async Task Maintenance_EndDateBeforeStartDate_Returns400()
    {
        var roomRepository =
            new Mock<IRoomRepository>();

        roomRepository
            .Setup(x => x.GetRoomByIdAsync(1))
            .ReturnsAsync(CreateActiveRoom());

        var controller =
            CreateMaintenanceController(
                roomRepository.Object,
                new Mock<IMaintenanceRepository>().Object,
                new Mock<IBookingRepository>().Object);

        SetAuthenticatedUser(
            controller,
            userId: 2001,
            role: "ADMIN");

        var request =
            CreateValidMaintenanceRequest();

        request.StartDate =
            DateOnly.FromDateTime(
                DateTime.Today.AddDays(5));

        request.EndDate =
            DateOnly.FromDateTime(
                DateTime.Today.AddDays(4));

        var result =
            await controller.CreateMaintenance(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Maintenance_InvalidPriority_Returns400()
    {
        var roomRepository =
            new Mock<IRoomRepository>();

        roomRepository
            .Setup(x => x.GetRoomByIdAsync(1))
            .ReturnsAsync(CreateActiveRoom());

        var maintenanceRepository =
            new Mock<IMaintenanceRepository>();

        maintenanceRepository
            .Setup(x =>
                x.HasMaintenanceScheduleConflictAsync(
                    It.IsAny<int>(),
                    It.IsAny<DateOnly>(),
                    It.IsAny<DateOnly?>()))
            .ReturnsAsync(false);

        var controller =
            CreateMaintenanceController(
                roomRepository.Object,
                maintenanceRepository.Object,
                new Mock<IBookingRepository>().Object);

        SetAuthenticatedUser(
            controller,
            userId: 2001,
            role: "ADMIN");

        var request =
            CreateValidMaintenanceRequest();

        request.PriorityLevel =
            "CRITICAL";

        var result =
            await controller.CreateMaintenance(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // =========================================================
    // CLEANING
    // =========================================================

    [Fact]
    public async Task Cleaning_InvalidDuration_Returns400()
    {
        var cleaningRepository =
            CreateCleaningRepository();

        var controller =
            CreateCleaningController(
                cleaningRepository.Object);

        SetAuthenticatedUser(
            controller,
            userId: 2001,
            role: "ADMIN");

        var request =
            new SetCleaningDurationRequestDto
            {
                CleaningDuration = "60_MINUTES",
                CustomDurationMinutes = null
            };

        var result =
            await controller.SetCleaningDuration(
                1,
                request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Cleaning_CustomWithoutMinutes_Returns400()
    {
        var cleaningRepository =
            CreateCleaningRepository();

        var controller =
            CreateCleaningController(
                cleaningRepository.Object);

        SetAuthenticatedUser(
            controller,
            userId: 2001,
            role: "ADMIN");

        var request =
            new SetCleaningDurationRequestDto
            {
                CleaningDuration = "CUSTOM",
                CustomDurationMinutes = null
            };

        var result =
            await controller.SetCleaningDuration(
                1,
                request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Cleaning_NonCustomWithCustomMinutes_Returns400()
    {
        var cleaningRepository =
            CreateCleaningRepository();

        var controller =
            CreateCleaningController(
                cleaningRepository.Object);

        SetAuthenticatedUser(
            controller,
            userId: 2001,
            role: "ADMIN");

        var request =
            new SetCleaningDurationRequestDto
            {
                CleaningDuration = "10_MINUTES",
                CustomDurationMinutes = 15
            };

        var result =
            await controller.SetCleaningDuration(
                1,
                request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // =========================================================
    // CONTROLLER FACTORIES
    // =========================================================

    private static BookingController CreateBookingController(
        IRoomRepository roomRepository)
    {
        return new BookingController(
            new Mock<IBookingRepository>().Object,
            roomRepository,
            new Mock<IUserRepository>().Object,
            new Mock<IBookingCancellationRepository>().Object,
            new Mock<INotificationRepository>().Object,
            new Mock<IMaintenanceRepository>().Object,

            // Tidak dipakai karena test berhenti
            // sebelum mencapai service.
            null!,

            null!);
    }

    private static MaintenanceController
        CreateMaintenanceController(
            IRoomRepository roomRepository,
            IMaintenanceRepository maintenanceRepository,
            IBookingRepository bookingRepository)
    {
        return new MaintenanceController(
            maintenanceRepository,
            roomRepository,
            bookingRepository,

            // Tidak dipakai pada invalid-request tests.
            null!);
    }

    private static RoomCleaningController
        CreateCleaningController(
            IRoomCleaningSessionRepository cleaningRepository)
    {
        return new RoomCleaningController(
            cleaningRepository,

            // Tidak dipakai karena validation
            // mengembalikan 400 lebih dahulu.
            null!);
    }

    // =========================================================
    // MOCK HELPERS
    // =========================================================

    private static Mock<IRoomCleaningSessionRepository>
        CreateCleaningRepository()
    {
        var repository =
            new Mock<IRoomCleaningSessionRepository>();

        repository
            .Setup(x =>
                x.GetCleaningSessionByIdAsync(1))
            .ReturnsAsync(
                new RoomCleaningSession
                {
                    Id = 1,
                    RoomId = 1,
                    BookingId = 1,
                    CleaningDuration = "20_MINUTES",
                    CustomDurationMinutes = null,
                    StartTime = DateTime.UtcNow,
                    ScheduledEndTime =
                        DateTime.UtcNow.AddMinutes(20),
                    EndTime = null,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                });

        return repository;
    }

    private static Room CreateActiveRoom()
    {
        return new Room
        {
            Id = 1,
            Name = "Automated Test Room",
            Location = "Test Location",
            Capacity = 50,
            Description = "Room for automated tests",
            ImageUrl = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static CreateBookingRequestDto
        CreateValidBookingRequest()
    {
        return new CreateBookingRequestDto
        {
            RoomId = 1,
            BookingDate =
                DateOnly.FromDateTime(
                    DateTime.Today.AddDays(1)),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            NumPeople = 10,
            Title = "Business Rule Test",
            RequesterName = "Test User",
            RequesterDivision = "IT",
            Description = "Automated regression test"
        };
    }

    private static CreateMaintenanceRequestDto
        CreateValidMaintenanceRequest()
    {
        return new CreateMaintenanceRequestDto
        {
            RoomId = 1,
            MaintenanceCategory = "GENERAL",
            PriorityLevel = "MEDIUM",
            Description = "Automated maintenance test",
            StartDate =
                DateOnly.FromDateTime(
                    DateTime.Today.AddDays(2)),
            EndDate =
                DateOnly.FromDateTime(
                    DateTime.Today.AddDays(3))
        };
    }

    // =========================================================
    // AUTH PRINCIPAL
    // =========================================================

    private static void SetAuthenticatedUser(
        ControllerBase controller,
        int userId,
        string role)
    {
        var claims =
            new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    "Automated Test User"),

                new Claim(
                    ClaimTypes.Role,
                    role)
            };

        var identity =
            new ClaimsIdentity(
                claims,
                "Test");

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User =
                            new ClaimsPrincipal(
                                identity)
                    }
            };
    }
}