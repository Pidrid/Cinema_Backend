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

namespace Cinema_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SeatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //  GET: api/seats
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SeatDto>>> GetAll(int? roomId = null)
        {
            var query = _context.Seats.AsQueryable();

            if (roomId.HasValue)
                query = query.Where(s => s.RoomId == roomId.Value);

            var seats = await query
                .Select(s => new SeatDto
                {
                    SeatId = s.SeatId,
                    RoomId = s.RoomId,
                    Row = s.Row,
                    Column = s.Column
                })
                .ToListAsync();

            return Ok(seats);
        }

        // GET: api/seats/occupied?screeningId=1
        [HttpGet("occupied")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SeatDto>>> GetOccupiedSeats(int screeningId)
        {
            var screeningExists = await _context.Screenings.AnyAsync(s => s.ScreeningId == screeningId);
            if (!screeningExists)
                return NotFound($"Screening with ID {screeningId} doesn't exist.");

            var occupiedSeatIds = await _context.ReservationSeats
                .Where(rs => rs.Reservation.ScreeningId == screeningId)
                .Select(rs => rs.SeatId)
                .Distinct()
                .ToListAsync();

            var occupiedSeats = await _context.Seats
                .Where(s => occupiedSeatIds.Contains(s.SeatId))
                .Select(s => new SeatDto
                {
                    SeatId = s.SeatId,
                    RoomId = s.RoomId,
                    Row = s.Row,
                    Column = s.Column
                })
                .ToListAsync();

            return Ok(occupiedSeats);
        }

        // GET: api/seats/available?screeningId=1
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SeatDto>>> GetAvailableSeats(int screeningId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.ScreeningId == screeningId);

            if (screening == null)
                return NotFound($"Screening with ID {screeningId} doesn't exist.");

            var occupiedSeatIds = await _context.ReservationSeats
                .Where(rs => rs.Reservation.ScreeningId == screeningId)
                .Select(rs => rs.SeatId)
                .Distinct()
                .ToListAsync();

            var availableSeats = await _context.Seats
                .Where(s => s.RoomId == screening.RoomId && !occupiedSeatIds.Contains(s.SeatId))
                .Select(s => new SeatDto
                {
                    SeatId = s.SeatId,
                    RoomId = s.RoomId,
                    Row = s.Row,
                    Column = s.Column
                })
                .ToListAsync();

            return Ok(availableSeats);
        }

        //  GET: api/seats/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SeatDto>> GetById(int id)
        {
            var seat = await _context.Seats
                .Where(s => s.SeatId == id)
                .Select(s => new SeatDto
                {
                    SeatId = s.SeatId,
                    RoomId = s.RoomId,
                    Row = s.Row,
                    Column = s.Column
                })
                .FirstOrDefaultAsync();

            if (seat == null)
                return NotFound();

            return Ok(seat);
        }

        //  POST: api/seats
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SeatDto>> Create([FromBody] SeatCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == dto.RoomId);
            if (!roomExists)
                return BadRequest($"Room with ID {dto.RoomId} doesn't exist.");

            var seat = new Seat
            {
                RoomId = dto.RoomId,
                Row = dto.Row,
                Column = dto.Column
            };

            _context.Seats.Add(seat);
            await _context.SaveChangesAsync();

            var resultDto = new SeatDto
            {
                SeatId = seat.SeatId,
                RoomId = seat.RoomId,
                Row = seat.Row,
                Column = seat.Column
            };

            return CreatedAtAction(nameof(GetById), new { id = seat.SeatId }, resultDto);
        }

        //  PUT: api/seats/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] SeatUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var seatInDb = await _context.Seats.FindAsync(id);
            if (seatInDb == null)
                return NotFound();

            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == dto.RoomId);
            if (!roomExists)
                return BadRequest($"Room with ID {dto.RoomId} doesn't exist.");

            seatInDb.RoomId = dto.RoomId;
            seatInDb.Row = dto.Row;
            seatInDb.Column = dto.Column;

            _context.Entry(seatInDb).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //  DELETE: api/seats/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var seatInDb = await _context.Seats.FindAsync(id);
            if (seatInDb == null)
                return NotFound();

            var isReserved = await _context.ReservationSeats
                .AnyAsync(rs => rs.SeatId == id);
            if (isReserved)
                return BadRequest("You can't delete this seat because it has been reserved.");

            _context.Seats.Remove(seatInDb);
            await _context.SaveChangesAsync();

            return NoContent();
        }
       
    }
}
