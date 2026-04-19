using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Member;
using LibraryManagementSystem.Services.Members;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] MemberFilterDto filter)
        {
            var result = await _memberService.GetAllAsync(filter);
            return Ok(PagedApiResponse<MemberDto>.SuccessResult(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var member = await _memberService.GetByIdAsync(id);
            if (member == null)
                return NotFound(ApiResponse<MemberDto>.FailResult($"Member with ID {id} not found."));
            return Ok(ApiResponse<MemberDto>.SuccessResult(member));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMemberDto dto)
        {
            var member = await _memberService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = member.Id },
                ApiResponse<MemberDto>.SuccessResult(member, "Member created successfully."));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateMemberDto dto)
        {
            var member = await _memberService.UpdateAsync(dto);
            if (member == null)
                return NotFound(ApiResponse<MemberDto>.FailResult("Data not found."));
            return Ok(ApiResponse<MemberDto>.SuccessResult(member, "Member updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _memberService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.FailResult($"Member with ID {id} not found."));
            return NoContent();
        }

        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _memberService.ActivateAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.FailResult($"Member with ID {id} not found."));
            return Ok(ApiResponse<object>.SuccessResult(null!, "Member activated successfully."));
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _memberService.DeactivateAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.FailResult($"Member with ID {id} not found."));
            return Ok(ApiResponse<object>.SuccessResult(null!, "Member deactivated successfully."));
        }
    }
}
