using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Library;

namespace LibraryManagementSystem.Services.Libraries
{
    public interface ILibraryService
    {
        Task<PagedResult<LibraryDto>> GetAllAsync(LibraryFilterDto filter);
        Task<LibraryDto?> GetByIdAsync(int id);
        Task<LibraryDto> CreateAsync(CreateLibraryDto dto);
        Task<LibraryDto?> UpdateAsync(UpdateLibraryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
