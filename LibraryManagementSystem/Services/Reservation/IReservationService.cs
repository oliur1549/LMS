using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Reservation;

namespace LibraryManagementSystem.Services.Reservations
{
    public interface IReservationService
    {
        Task<PagedResult<ReservationDto>> GetAllAsync(ReservationFilterDto filter);
        Task<ReservationDto?> GetByIdAsync(int id);
        Task<ReservationDto> CreateAsync(CreateReservationDto dto);
        Task<ReservationDto?> CancelAsync(int id);
        Task<ReservationDto?> FulfilAsync(int id);
    }
}
