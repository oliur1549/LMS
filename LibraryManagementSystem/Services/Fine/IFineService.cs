using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Fine;

namespace LibraryManagementSystem.Services.Fines
{
    public interface IFineService
    {
        Task<PagedResult<FineDto>> GetAllAsync(FineFilterDto filter);
        Task<FineDto?> GetByIdAsync(int id);
        Task<FineDto> IssueAsync(CreateFineDto dto);
        Task<FineDto?> PayAsync(int id);
    }
}
