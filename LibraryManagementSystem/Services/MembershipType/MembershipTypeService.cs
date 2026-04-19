using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.MembershipType;
using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services.MembershipTypes
{
    public class MembershipTypeService : IMembershipTypeService
    {
        private readonly IRepository<MembershipType> _repository;
        private readonly ApplicationDbContext _context;

        public MembershipTypeService(IRepository<MembershipType> repository, ApplicationDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<PagedResult<MembershipTypeDto>> GetAllAsync(MembershipTypeFilterDto filter)
        {
            var query = _context.MembershipTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(mt => mt.Name.Contains(filter.Name));

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<MembershipTypeDto>
            {
                Data = items.Select(MapToDto),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<MembershipTypeDto?> GetByIdAsync(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            return type == null ? null : MapToDto(type);
        }

        public async Task<MembershipTypeDto> CreateAsync(CreateMembershipTypeDto dto)
        {
            var type = new MembershipType
            {
                Name = dto.Name,
                MaxBooksAllowed = dto.MaxBooksAllowed,
                BorrowDurationDays = dto.BorrowDurationDays
            };

            var created = await _repository.AddAsync(type);
            return MapToDto(created);
        }

        public async Task<MembershipTypeDto?> UpdateAsync(UpdateMembershipTypeDto dto)
        {
            var type = await _repository.GetByIdAsync(dto.Id);
            if (type == null) return null;

            type.Name = dto.Name;
            type.MaxBooksAllowed = dto.MaxBooksAllowed;
            type.BorrowDurationDays = dto.BorrowDurationDays;

            await _repository.UpdateAsync(type);
            return MapToDto(type);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _repository.ExistsAsync(id)) return false;
            await _repository.DeleteAsync(id);
            return true;
        }

        private static MembershipTypeDto MapToDto(MembershipType type) => new()
        {
            Id = type.Id,
            Name = type.Name,
            MaxBooksAllowed = type.MaxBooksAllowed,
            BorrowDurationDays = type.BorrowDurationDays
        };
    }
}
