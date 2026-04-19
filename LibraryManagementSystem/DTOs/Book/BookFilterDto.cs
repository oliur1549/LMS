using LibraryManagementSystem.DTOs.Common;

namespace LibraryManagementSystem.DTOs.Book
{
    public class BookFilterDto : PagedRequest
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Genre { get; set; }
        public int? LibraryId { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
