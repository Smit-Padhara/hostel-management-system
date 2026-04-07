using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Service interface for room-related operations
/// </summary>
public interface IRoomService
{
    /// <summary>
    /// Create a new room in a hostel
    /// </summary>
    /// <param name="request">Room creation request</param>
    /// <returns>Created room DTO</returns>
    Task<RoomDto> CreateRoomAsync(CreateRoomRequest request);

    /// <summary>
    /// Get room by ID
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <returns>Room DTO if found</returns>
    Task<RoomDto?> GetRoomByIdAsync(int roomId);

    /// <summary>
    /// Get all rooms in a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>List of rooms in the hostel</returns>
    Task<IEnumerable<RoomDto>> GetRoomsByHostelAsync(int hostelId);

    /// <summary>
    /// Get available rooms in a hostel (rooms with empty seats)
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>List of available rooms</returns>
    Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(int hostelId);

    /// <summary>
    /// Check if a room has capacity for new students
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <returns>Available seats in the room</returns>
    Task<int> CheckRoomCapacityAsync(int roomId);

    /// <summary>
    /// Check if a room is at full capacity
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <returns>True if room is full</returns>
    Task<bool> IsRoomFullAsync(int roomId);

    /// <summary>
    /// Increment room occupancy (when student is assigned)
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <returns>New occupancy count</returns>
    Task<int> IncrementOccupancyAsync(int roomId);

    /// <summary>
    /// Decrement room occupancy (when student is removed)
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <returns>New occupancy count</returns>
    Task<int> DecrementOccupancyAsync(int roomId);

    /// <summary>
    /// Update room information
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated room DTO</returns>
    Task<RoomDto?> UpdateRoomAsync(int roomId, CreateRoomRequest request);

    /// <summary>
    /// Delete room (soft delete)
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteRoomAsync(int roomId);
}
