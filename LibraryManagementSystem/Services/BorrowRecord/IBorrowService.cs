using LibraryManagementSystem.DTOs.BorrowRecord;
using LibraryManagementSystem.DTOs.Common;

namespace LibraryManagementSystem.Services.BorrowRecords
{
    public interface IBorrowService
    {
        Task<PagedResult<BorrowRecordDto>> GetAllAsync(BorrowRecordFilterDto filter);
        Task<BorrowRecordDto?> GetByIdAsync(int id);
        Task<BorrowRecordDto> BorrowBookAsync(BorrowBookDto dto);
        Task<BorrowRecordDto?> ReturnBookAsync(int borrowRecordId);
    }
}
