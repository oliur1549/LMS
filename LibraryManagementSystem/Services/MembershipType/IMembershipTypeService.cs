using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.MembershipType;

namespace LibraryManagementSystem.Services.MembershipTypes
{
    public interface IMembershipTypeService
    {
        Task<PagedResult<MembershipTypeDto>> GetAllAsync(MembershipTypeFilterDto filter);
        Task<MembershipTypeDto?> GetByIdAsync(int id);
        Task<MembershipTypeDto> CreateAsync(CreateMembershipTypeDto dto);
        Task<MembershipTypeDto?> UpdateAsync(UpdateMembershipTypeDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
