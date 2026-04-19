using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Member;
using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services.Members
{
    public class MemberService : IMemberService
    {
        private readonly IRepository<Member> _memberRepository;
        private readonly ApplicationDbContext _context;

        public MemberService(IRepository<Member> memberRepository, ApplicationDbContext context)
        {
            _memberRepository = memberRepository;
            _context = context;
        }

        public async Task<PagedResult<MemberDto>> GetAllAsync(MemberFilterDto filter)
        {
            var query = _context.Members.Include(m => m.MembershipType).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.FullName))
                query = query.Where(m => m.FullName.Contains(filter.FullName));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                query = query.Where(m => m.Email.Contains(filter.Email));

            if (filter.IsActive.HasValue)
                query = query.Where(m => m.IsActive == filter.IsActive.Value);

            if (filter.MembershipTypeId.HasValue)
                query = query.Where(m => m.MembershipTypeId == filter.MembershipTypeId.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<MemberDto>
            {
                Data = items.Select(MapToDto),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<MemberDto?> GetByIdAsync(int id)
        {
            var member = await _context.Members.Include(m => m.MembershipType)
                                               .FirstOrDefaultAsync(m => m.Id == id);
            return member == null ? null : MapToDto(member);
        }

        public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
        {
            var member = new Member
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                MembershipTypeId = dto.MembershipTypeId
            };

            var created = await _memberRepository.AddAsync(member);
            return await GetByIdAsync(created.Id) ?? MapToDto(created);
        }

        public async Task<MemberDto?> UpdateAsync(UpdateMemberDto dto)
        {
            var member = await _memberRepository.GetByIdAsync(dto.Id);
            if (member == null) return null;

            member.FullName = dto.FullName;
            member.Phone = dto.Phone;
            member.Address = dto.Address;
            member.MembershipTypeId = dto.MembershipTypeId;

            await _memberRepository.UpdateAsync(member);
            return await GetByIdAsync(member.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _memberRepository.ExistsAsync(id)) return false;
            await _memberRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var member = await _memberRepository.GetByIdAsync(id);
            if (member == null) return false;
            member.Activate();
            await _memberRepository.UpdateAsync(member);
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var member = await _memberRepository.GetByIdAsync(id);
            if (member == null) return false;
            member.Deactivate();
            await _memberRepository.UpdateAsync(member);
            return true;
        }

        private static MemberDto MapToDto(Member member) => new()
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            Phone = member.Phone,
            Address = member.Address,
            MembershipDate = member.MembershipDate,
            IsActive = member.IsActive,
            MembershipTypeId = member.MembershipTypeId,
            MembershipTypeName = member.MembershipType?.Name ?? string.Empty
        };
    }
}
