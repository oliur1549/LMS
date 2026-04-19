using LibraryManagementSystem.DTOs.Common;

namespace LibraryManagementSystem.DTOs.MembershipType
{
    public class MembershipTypeFilterDto : PagedRequest
    {
        public string? Name { get; set; }
    }
}
