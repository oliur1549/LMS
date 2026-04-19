using LibraryManagementSystem.DTOs.Common;

namespace LibraryManagementSystem.DTOs.Fine
{
    public class FineFilterDto : PagedRequest
    {
        public int? MemberId { get; set; }
        public bool? IsPaid { get; set; }
    }
}
