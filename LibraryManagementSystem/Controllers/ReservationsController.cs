using LibraryManagementSystem.DTOs.Common;
using LibraryManagementSystem.DTOs.Reservation;
using LibraryManagementSystem.Services.Reservations;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ReservationFilterDto filter)
        {
            var result = await _reservationService.GetAllAsync(filter);
            return Ok(PagedApiResponse<ReservationDto>.SuccessResult(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _reservationService.GetByIdAsync(id);
            if (reservation == null)
                return NotFound(ApiResponse<ReservationDto>.FailResult("Data not found."));
            return Ok(ApiResponse<ReservationDto>.SuccessResult(reservation));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            var reservation = await _reservationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = reservation.Id },
                ApiResponse<ReservationDto>.SuccessResult(reservation, "Reservation created successfully."));
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _reservationService.CancelAsync(id);
            if (reservation == null)
                return NotFound(ApiResponse<ReservationDto>.FailResult("Data not found."));
            return Ok(ApiResponse<ReservationDto>.SuccessResult(reservation, "Reservation cancelled successfully."));
        }

        [HttpPatch("{id}/fulfil")]
        public async Task<IActionResult> Fulfil(int id)
        {
            var reservation = await _reservationService.FulfilAsync(id);
            if (reservation == null)
                return NotFound(ApiResponse<ReservationDto>.FailResult("Data not found."));
            return Ok(ApiResponse<ReservationDto>.SuccessResult(reservation, "Reservation fulfilled successfully."));
        }
    }
}
