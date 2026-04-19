using LibraryManagementSystem.DTOs.Common;

namespace LibraryManagementSystem.DTOs.Library
{
    public class LibraryFilterDto : PagedRequest
    {
        public string? Name { get; set; }
    }
}
