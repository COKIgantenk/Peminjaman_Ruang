using Dapper;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public BookingRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT
                        id AS ""Id"",
                        user_id AS ""UserId"",
                        room_id AS ""RoomId"",
                        booking_date AS ""BookingDate"",
                        start_time AS ""StartTime"",
                        end_time AS ""EndTime"",
                        num_people AS ""NumPeople"",
                        title AS ""Title"",
                        requester_name AS ""RequesterName"",
                        requester_division AS ""RequesterDivision"",
                        description AS ""Description"",
                        status AS ""Status"",
                        approval_notes AS ""ApprovalNotes"",
                        approved_by_admin_id AS ""ApprovedByAdminId"",
                        created_at AS ""CreatedAt"",
                        updated_at AS ""UpdatedAt""
                    FROM bookings
                    ORDER BY booking_date DESC, start_time DESC";

                return await connection.QueryAsync<Booking>(query);
            }
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT
                        id AS ""Id"",
                        user_id AS ""UserId"",
                        room_id AS ""RoomId"",
                        booking_date AS ""BookingDate"",
                        start_time AS ""StartTime"",
                        end_time AS ""EndTime"",
                        num_people AS ""NumPeople"",
                        title AS ""Title"",
                        requester_name AS ""RequesterName"",
                        requester_division AS ""RequesterDivision"",
                        description AS ""Description"",
                        status AS ""Status"",
                        approval_notes AS ""ApprovalNotes"",
                        approved_by_admin_id AS ""ApprovedByAdminId"",
                        created_at AS ""CreatedAt"",
                        updated_at AS ""UpdatedAt""
                    FROM bookings
                    WHERE id = @Id";

                return await connection.QueryFirstOrDefaultAsync<Booking>(
                    query, 
                    new { Id = id });
            }
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT
                        id AS ""Id"",
                        user_id AS ""UserId"",
                        room_id AS ""RoomId"",
                        booking_date AS ""BookingDate"",
                        start_time AS ""StartTime"",
                        end_time AS ""EndTime"",
                        num_people AS ""NumPeople"",
                        title AS ""Title"",
                        requester_name AS ""RequesterName"",
                        requester_division AS ""RequesterDivision"",
                        description AS ""Description"",
                        status AS ""Status"",
                        approval_notes AS ""ApprovalNotes"",
                        approved_by_admin_id AS ""ApprovedByAdminId"",
                        created_at AS ""CreatedAt"",
                        updated_at AS ""UpdatedAt""
                    FROM bookings
                    WHERE user_id = @UserId";

                return await connection.QueryAsync<Booking>(
                    query, 
                    new { UserId = userId });
            }
        }

        public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(string status)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT
                        id AS ""Id"",
                        user_id AS ""UserId"",
                        room_id AS ""RoomId"",
                        booking_date AS ""BookingDate"",
                        start_time AS ""StartTime"",
                        end_time AS ""EndTime"",
                        num_people AS ""NumPeople"",
                        title AS ""Title"",
                        requester_name AS ""RequesterName"",
                        requester_division AS ""RequesterDivision"",
                        description AS ""Description"",
                        status AS ""Status"",
                        approval_notes AS ""ApprovalNotes"",
                        approved_by_admin_id AS ""ApprovedByAdminId"",
                        created_at AS ""CreatedAt"",
                        updated_at AS ""UpdatedAt""
                    FROM bookings
                    WHERE status = @Status
                    ORDER BY booking_date DESC, start_time DESC ";

                return await connection.QueryAsync<Booking>(
                    query, 
                    new { Status = status });
            }
        }

        public async Task<IEnumerable<Booking>> GetBookingsByDateAsync(DateOnly date)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT
                        id AS ""Id"",
                        user_id AS ""UserId"",
                        room_id AS ""RoomId"",
                        booking_date AS ""BookingDate"",
                        start_time AS ""StartTime"",
                        end_time AS ""EndTime"",
                        num_people AS ""NumPeople"",
                        title AS ""Title"",
                        requester_name AS ""RequesterName"",
                        requester_division AS ""RequesterDivision"",
                        description AS ""Description"",
                        status AS ""Status"",
                        approval_notes AS ""ApprovalNotes"",
                        approved_by_admin_id AS ""ApprovedByAdminId"",
                        created_at AS ""CreatedAt"",
                        updated_at AS ""UpdatedAt""
                    FROM bookings
                    WHERE booking_date = @Date ORDER BY start_time";

                return await connection.QueryAsync<Booking>(
                    query, 
                    new { Date = date });
            }
        }

        public async Task<int> CreateBookingAsync(Booking booking)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    INSERT INTO bookings
                    (
                        user_id,
                        room_id,
                        booking_date,
                        start_time,
                        end_time,
                        num_people,
                        title,
                        requester_name,
                        requester_division,
                        description,
                        status,
                        approval_notes,
                        approved_by_admin_id,
                        created_at,
                        updated_at
                    )
                    VALUES
                    (
                        @UserId,
                        @RoomId,
                        @BookingDate,
                        @StartTime,
                        @EndTime,
                        @NumPeople,
                        @Title,
                        @RequesterName,
                        @RequesterDivision,
                        @Description,
                        @Status,
                        @ApprovalNotes,
                        @ApprovedByAdminId,
                        NOW(),
                        NOW()
                    )
                    RETURNING id";
                
                return await connection.ExecuteScalarAsync<int>(
                    query, 
                    booking);
            }
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    UPDATE bookings 
                    SET booking_date = @BookingDate, start_time = @StartTime, end_time = @EndTime,
                        num_people = @NumPeople, title = @Title, requester_name = @RequesterName,
                        requester_division = @RequesterDivision, description = @Description,
                        status = @Status, approval_notes = @ApprovalNotes, updated_at = NOW()
                    WHERE id = @Id";
                
                var result = await connection.ExecuteAsync(query, booking);
                return result > 0;
            }
        }

        public async Task<bool> ApproveBookingAsync(int bookingId, int adminId)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    UPDATE bookings 
                    SET 
                        status = 'APPROVED', 
                        approved_by_admin_id = @AdminId, 
                        updated_at = NOW()
                    WHERE id = @BookingId
                        AND status = 'PENDING'";
                
                var result = await connection.ExecuteAsync(
                    query, 
                    new 
                    {
                        BookingId = bookingId,
                        AdminId = adminId
                    });  

                return result > 0;
            }
        }

        public async Task<bool> RejectBookingAsync(
            int bookingId, 
            int adminId, 
            string reason)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    UPDATE bookings 
                    SET 
                        status = 'REJECTED', 
                        approval_notes = @Reason, 
                        approved_by_admin_id = @AdminId, 
                        updated_at = NOW()
                    WHERE id = @BookingId
                        AND status = 'PENDING'";
                
                var result = await connection.ExecuteAsync(
                    query, 
                    new 
                    { 
                        BookingId = bookingId, 
                        AdminId = adminId, 
                        Reason = reason 
                    });

                return result > 0;
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    UPDATE bookings 
                    SET 
                        status = 'CANCELLED', 
                        updated_at = NOW()
                    WHERE id = @Id
                        AND status IN ('PENDING', 'APPROVED')";

                var result = await connection.ExecuteAsync(
                    query, 
                    new { Id = bookingId });

                return result > 0;
            }
        }

        public async Task<bool> HasBookingConflictAsync(
            int roomId, 
            DateOnly bookingDate, 
            TimeOnly startTime, 
            TimeOnly endTime,
            int? excludebookingId = null)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT EXISTS (
                        SELECT 1
                        FROM bookings
                        WHERE room_id = @RoomId
                            AND booking_date = @BookingDate
                            AND status IN ('PENDING', 'APPROVED')
                            AND (@ExcludeBookingId IS NULL OR id != @ExcludeBookingId)
                            AND start_time < @EndTime
                            AND end_time > @StartTime
                        )";

            return await connection.ExecuteScalarAsync<bool>(
                query, 
                new 
                { 
                    RoomId = roomId, 
                    BookingDate = bookingDate, 
                    StartTime = startTime, 
                    EndTime = endTime,
                    ExcludeBookingId = excludebookingId
                });

            }
        }

        public async Task<bool> IsRoomCurrentlyInUseAsync(int roomId)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM bookings
                    WHERE room_id = @RoomId
                        AND status = 'APPROVED'
                        AND booking_date = CURRENT_DATE
                        AND start_time <= CURRENT_TIME
                        AND end_time > CURRENT_TIME
                )";

            return await connection.ExecuteScalarAsync<bool>(
                query,
                new { RoomId = roomId });
        }

        public async Task<bool> HasBookingConflictInDateRangeAsync(
            int roomId,
            DateOnly startDate,
            DateOnly? endDate)
        {
            using var connection = _dbConnection.CreateConnection();

            var effectiveEndDate = endDate ?? startDate;

            const string query = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM bookings
                    WHERE room_id = @RoomId
                        AND status IN ('PENDING', 'APPROVED')
                        AND booking_date >= @StartDate
                        AND booking_date <= @EndDate
                )";

            return await connection.ExecuteScalarAsync<bool>(
                query,
                new
                {
                    RoomId = roomId,
                    StartDate = startDate,
                    EndDate = effectiveEndDate
                });   
                 
        }

        public async Task<IEnumerable<Booking>> GetFinishedBookingsWithoutCleaningAsync()
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    b.id AS ""Id"",
                    b.user_id AS ""UserId"",
                    b.room_id AS ""RoomId"",
                    b.booking_date AS ""BookingDate"",
                    b.start_time AS ""StartTime"",
                    b.end_time AS ""EndTime"",
                    b.num_people AS ""NumPeople"",
                    b.title AS ""Title"",
                    b.requester_name AS ""RequesterName"",
                    b.requester_division AS ""RequesterDivision"",
                    b.description AS ""Description"",
                    b.status AS ""Status"",
                    b.approval_notes AS ""ApprovalNotes"",
                    b.approved_by_admin_id AS ""ApprovedByAdminId"",
                    b.created_at AS ""CreatedAt"",
                    b.updated_at AS ""UpdatedAt""
                FROM bookings b
                WHERE b.status = 'APPROVED'
                  AND (
                        b.booking_date < 
                            timezone('Asia/Jakarta', CURRENT_TIMESTAMP):: date  

                        OR (
                            b.booking_date = 
                                timezone('Asia/Jakarta', CURRENT_TIMESTAMP):: date
                            
                            AND b.end_time <= 
                                timezone('Asia/Jakarta', CURRENT_TIMESTAMP):: time
                           )
                        )
                  AND NOT EXISTS (
                        SELECT 1
                        FROM room_cleaning_session rcs
                        WHERE rcs.booking_id = b.id
                  )
                ORDER BY b.booking_date, b.end_time";

            return await connection.QueryAsync<Booking>(query);
        }
    }
}