using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.MembershipType;
using LibraryManagementSystem.Services.MembershipTypes;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipTypesController : ControllerBase
    {
        private readonly IMembershipTypeService _membershipTypeService;

        public MembershipTypesController(IMembershipTypeService membershipTypeService)
        {
            _membershipTypeService = membershipTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] MembershipTypeFilterDto filter)
        {
            var result = await _membershipTypeService.GetAllAsync(filter);
            return Ok(PagedApiResponse<MembershipTypeDto>.SuccessResult(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var type = await _membershipTypeService.GetByIdAsync(id);
            if (type == null)
                return NotFound(ApiResponse<MembershipTypeDto>.FailResult("Data not found."));
            return Ok(ApiResponse<MembershipTypeDto>.SuccessResult(type));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMembershipTypeDto dto)
        {
            var type = await _membershipTypeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = type.Id },
                ApiResponse<MembershipTypeDto>.SuccessResult(type, "Membership type created successfully."));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateMembershipTypeDto dto)
        {
            var type = await _membershipTypeService.UpdateAsync(dto);
            if (type == null)
                return NotFound(ApiResponse<MembershipTypeDto>.FailResult("Data not found."));
            return Ok(ApiResponse<MembershipTypeDto>.SuccessResult(type, "Membership type updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _membershipTypeService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.FailResult("Data not found."));
            return NoContent();
        }
    }
}
