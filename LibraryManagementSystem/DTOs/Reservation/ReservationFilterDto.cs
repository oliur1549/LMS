using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.Enums;

namespace LibraryManagementSystem.DTOs.Reservation
{
    public class ReservationFilterDto : PagedRequest
    {
        public int? MemberId { get; set; }
        public int? BookId { get; set; }
        public ReservationStatus? Status { get; set; }
    }
}
