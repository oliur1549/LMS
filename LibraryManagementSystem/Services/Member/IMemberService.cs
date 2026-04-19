using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Member;

namespace LibraryManagementSystem.Services.Members
{
    public interface IMemberService
    {
        Task<PagedResult<MemberDto>> GetAllAsync(MemberFilterDto filter);
        Task<MemberDto?> GetByIdAsync(int id);
        Task<MemberDto> CreateAsync(CreateMemberDto dto);
        Task<MemberDto?> UpdateAsync(UpdateMemberDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
