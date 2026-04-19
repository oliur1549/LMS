using LibraryManagementSystem.DTOs.Common;

namespace LibraryManagementSystem.DTOs.Member
{
    public class MemberFilterDto : PagedRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public int? MembershipTypeId { get; set; }
    }
}
