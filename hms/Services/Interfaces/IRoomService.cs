using SmartHostelManagementSystem.Models.DTOs;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Interface for room service
/// </summary>
public interface IRoomService
{
    /// <summary>
    /// Create a new room
    /// </summary>
    Task<RoomDto?> CreateRoomAsync(CreateRoomRequest request);
    
    /// <summary>
    /// Get all rooms for a hostel
    /// </summary>
    Task<List<RoomDto>> GetAllRoomsAsync(int hostelId);
    
    /// <summary>
    /// Get room by ID
    /// </summary>
    Task<RoomDto?> GetRoomByIdAsync(int roomId, int hostelId);
    
    /// <summary>
    /// Get available rooms
    /// </summary>
    Task<List<RoomAvailabilityDto>> GetAvailableRoomsAsync(int hostelId);
    
    /// <summary>
    /// Update room
    /// </summary>
    Task<RoomDto?> UpdateRoomAsync(int roomId, int hostelId, CreateRoomRequest request);
    
    /// <summary>
    /// Delete room (soft delete)
    /// </summary>
    Task<bool> DeleteRoomAsync(int roomId, int hostelId);
}
