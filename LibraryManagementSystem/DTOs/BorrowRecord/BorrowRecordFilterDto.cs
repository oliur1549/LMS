using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.Enums;

namespace LibraryManagementSystem.DTOs.BorrowRecord
{
    public class BorrowRecordFilterDto : PagedRequest
    {
        public int? MemberId { get; set; }
        public int? BookId { get; set; }
        public BorrowStatus? Status { get; set; }
    }
}
