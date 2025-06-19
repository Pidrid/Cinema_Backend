using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cinema_Backend.Data;
using Cinema_Backend.Models;
using Cinema_Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Cinema_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationSeatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservationSeatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //  GET: api/reservationseats
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReservationSeatDto>>> GetAll()
        {
            var all = await _context.ReservationSeats
                .Select(rs => new ReservationSeatDto
                {
                    ReservationSeatId = rs.ReservationSeatId,
                    ReservationId = rs.ReservationId,
                    SeatId = rs.SeatId
                })
                .ToListAsync();

            return Ok(all);
        }

        //  GET: api/reservationseats/reservation/{reservationId}
        //  If Admin: return all seats for the reservation
        //  If user: return only if they own the reservation
        [HttpGet("reservation/{reservationId:int}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReservationSeatDto>>> GetByReservation(int reservationId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
                return Forbid();

            var reservation = await _context.Reservations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null)
                return NotFound($"Nie znaleziono rezerwacji o ID {reservationId}.");

            if (!User.IsInRole("Admin") && reservation.UserId != userIdString)
                return Forbid(); // 403

            var list = await _context.ReservationSeats
                .Where(rs => rs.ReservationId == reservationId)
                .Select(rs => new ReservationSeatDto
                {
                    ReservationSeatId = rs.ReservationSeatId,
                    ReservationId = rs.ReservationId,
                    SeatId = rs.SeatId
                })
                .ToListAsync();

            return Ok(list);
        }

        // ------------------------------------
        //  GET: api/reservationseats/{id}
        //  If Admin: return ReservationSeat by ID
        //  If user: return only if they own the reservation
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<ReservationSeatDto>> GetById(int id)
        {
            var rs = await _context.ReservationSeats
                .FirstOrDefaultAsync(x => x.ReservationSeatId == id);

            if (rs == null)
                return NotFound();

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
                return Forbid();

            if (!User.IsInRole("Admin"))
            {
                var reservation = await _context.Reservations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ReservationId == rs.ReservationId);

                if (reservation == null || reservation.UserId != userIdString)
                    return Forbid();
            }

            var dto = new ReservationSeatDto
            {
                ReservationSeatId = rs.ReservationSeatId,
                ReservationId = rs.ReservationId,
                SeatId = rs.SeatId
            };

            return Ok(dto);
        }

        //  POST: api/reservationseats
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ReservationSeatDto>> Create([FromBody] ReservationSeatCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reservationExists = await _context.Reservations
                .AnyAsync(r => r.ReservationId == dto.ReservationId);
            if (!reservationExists)
                return BadRequest($"Reservation with ID {dto.ReservationId} doesn't exist.");

            var seatExists = await _context.Seats
                .AnyAsync(s => s.SeatId == dto.SeatId);
            if (!seatExists)
                return BadRequest($"Seat with ID {dto.SeatId} doesn't exist.");

            // Check if the pair ReservationId + SeatId already exists
            var duplicate = await _context.ReservationSeats
                .AnyAsync(rs => rs.ReservationId == dto.ReservationId && rs.SeatId == dto.SeatId);
            if (duplicate)
                return BadRequest("Pair ReservationId + SeatId already exists.");

            var rs = new ReservationSeat
            {
                ReservationId = dto.ReservationId,
                SeatId = dto.SeatId
            };

            _context.ReservationSeats.Add(rs);
            await _context.SaveChangesAsync();

            var resultDto = new ReservationSeatDto
            {
                ReservationSeatId = rs.ReservationSeatId,
                ReservationId = rs.ReservationId,
                SeatId = rs.SeatId
            };

            return CreatedAtAction(nameof(GetById), new { id = rs.ReservationSeatId }, resultDto);
        }

        //  PUT: api/reservationseats/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ReservationSeatUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var rsInDb = await _context.ReservationSeats.FindAsync(id);
            if (rsInDb == null)
                return NotFound();

            var reservationExists = await _context.Reservations
                .AnyAsync(r => r.ReservationId == dto.ReservationId);
            if (!reservationExists)
                return BadRequest($"Reservation with ID {dto.ReservationId} doesn't exist.");

            var seatExists = await _context.Seats
                .AnyAsync(s => s.SeatId == dto.SeatId);
            if (!seatExists)
                return BadRequest($"Seat with ID {dto.SeatId} doesn't exist.");

            var duplicate = await _context.ReservationSeats
                .AnyAsync(x => x.ReservationId == dto.ReservationId
                               && x.SeatId == dto.SeatId
                               && x.ReservationSeatId != id);
            if (duplicate)
                return BadRequest("Pair ReservationId + SeatId already exists");

            rsInDb.ReservationId = dto.ReservationId;
            rsInDb.SeatId = dto.SeatId;

            _context.Entry(rsInDb).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //  DELETE: api/reservationseats/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var rsInDb = await _context.ReservationSeats.FindAsync(id);
            if (rsInDb == null)
                return NotFound();

            _context.ReservationSeats.Remove(rsInDb);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
