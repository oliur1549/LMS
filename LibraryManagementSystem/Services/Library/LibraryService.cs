using LibraryManagementSystem.Data;
using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Library;
using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services.Libraries
{
    public class LibraryService : ILibraryService
    {
        private readonly IRepository<Library> _libraryRepository;
        private readonly ApplicationDbContext _context;

        public LibraryService(IRepository<Library> libraryRepository, ApplicationDbContext context)
        {
            _libraryRepository = libraryRepository;
            _context = context;
        }

        public async Task<PagedResult<LibraryDto>> GetAllAsync(LibraryFilterDto filter)
        {
            var query = _context.Libraries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(l => l.Name.Contains(filter.Name));

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<LibraryDto>
            {
                Data = items.Select(MapToDto),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<LibraryDto?> GetByIdAsync(int id)
        {
            var library = await _libraryRepository.GetByIdAsync(id);
            return library == null ? null : MapToDto(library);
        }

        public async Task<LibraryDto> CreateAsync(CreateLibraryDto dto)
        {
            var library = new Library
            {
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email
            };

            var created = await _libraryRepository.AddAsync(library);
            return MapToDto(created);
        }

        public async Task<LibraryDto?> UpdateAsync(UpdateLibraryDto dto)
        {
            var library = await _libraryRepository.GetByIdAsync(dto.Id);
            if (library == null) return null;

            library.Name = dto.Name;
            library.Address = dto.Address;
            library.Phone = dto.Phone;
            library.Email = dto.Email;

            await _libraryRepository.UpdateAsync(library);
            return MapToDto(library);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _libraryRepository.ExistsAsync(id)) return false;
            await _libraryRepository.DeleteAsync(id);
            return true;
        }

        private static LibraryDto MapToDto(Library library) => new()
        {
            Id = library.Id,
            Name = library.Name,
            Address = library.Address,
            Phone = library.Phone,
            Email = library.Email
        };
    }
}
