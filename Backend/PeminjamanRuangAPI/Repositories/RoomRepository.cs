using Dapper;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public RoomRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = "SELECT * FROM rooms ORDER BY id";
                return await connection.QueryAsync<Room>(query);
            }
        }

        public async Task<Room> GetRoomByIdAsync(int id)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = "SELECT * FROM rooms WHERE id = @Id";
                return await connection.QueryFirstOrDefaultAsync<Room>(query, new { Id = id });
            }
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime bookingDate, TimeSpan startTime, TimeSpan endTime, int capacity)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT DISTINCT r.* 
                    FROM rooms r
                    WHERE r.is_active = true 
                    AND r.capacity >= @Capacity
                    AND r.id NOT IN (
                        SELECT room_id FROM bookings 
                        WHERE booking_date = @BookingDate 
                        AND status IN ('PENDING', 'APPROVED')
                        AND NOT (end_time <= @StartTime OR start_time >= @EndTime)
                    )
                    ORDER BY r.name";
                
                return await connection.QueryAsync<Room>(query, new 
                { 
                    BookingDate = bookingDate, 
                    StartTime = startTime, 
                    EndTime = endTime,
                    Capacity = capacity
                });
            }
        }

        public async Task<bool> CreateRoomAsync(Room room)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    INSERT INTO rooms (name, location, capacity, description, image_url, is_active, created_at, updated_at)
                    VALUES (@Name, @Location, @Capacity, @Description, @ImageUrl, @IsActive, NOW(), NOW())";
                
                var result = await connection.ExecuteAsync(query, room);
                return result > 0;
            }
        }

        public async Task<bool> UpdateRoomAsync(Room room)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    UPDATE rooms 
                    SET name = @Name, location = @Location, capacity = @Capacity, 
                        description = @Description, image_url = @ImageUrl, is_active = @IsActive, 
                        updated_at = NOW()
                    WHERE id = @Id";
                
                var result = await connection.ExecuteAsync(query, room);
                return result > 0;
            }
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = "UPDATE rooms SET is_active = false, updated_at = NOW() WHERE id = @Id";
                var result = await connection.ExecuteAsync(query, new { Id = id });
                return result > 0;
            }
        }

        public async Task<IEnumerable<Facility>> GetRoomFacilitiesAsync(int roomId)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT f.* FROM facilities f
                    INNER JOIN room_facilities rf ON f.id = rf.facility_id
                    WHERE rf.room_id = @RoomId
                    ORDER BY f.name";
                
                return await connection.QueryAsync<Facility>(query, new { RoomId = roomId });
            }
        }

        public async Task<bool> AddFacilityToRoomAsync(int roomId, int facilityId)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    INSERT INTO room_facilities (room_id, facility_id, created_at)
                    VALUES (@RoomId, @FacilityId, NOW())
                    ON CONFLICT DO NOTHING";
                
                var result = await connection.ExecuteAsync(query, new { RoomId = roomId, FacilityId = facilityId });
                return result > 0;
            }
        }

        public async Task<bool> RemoveFacilityFromRoomAsync(int roomId, int facilityId)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = "DELETE FROM room_facilities WHERE room_id = @RoomId AND facility_id = @FacilityId";
                var result = await connection.ExecuteAsync(query, new { RoomId = roomId, FacilityId = facilityId });
                return result > 0;
            }
        }
    }
}