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

        // ------------------------------------
        // 1) GET: api/seats
        //    -- dostępne dla zalogowanych użytkowników
        //    -- opcjonalnie można wymusić parametry: ?roomId=1
        [HttpGet]
        [Authorize]
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

        // ------------------------------------
        // 2) GET: api/seats/{id}
        //    -- dostępne dla zalogowanych użytkowników
        [HttpGet("{id:int}")]
        [Authorize]
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

        // ------------------------------------
        // 3) POST: api/seats
        //    -- dostępny tylko dla Admina
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SeatDto>> Create([FromBody] SeatCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Sprawdź, czy sala istnieje
            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == dto.RoomId);
            if (!roomExists)
                return BadRequest($"Sala o ID {dto.RoomId} nie istnieje.");

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

        // ------------------------------------
        // 4) PUT: api/seats/{id}
        //    -- dostępny tylko dla Admina
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] SeatUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var seatInDb = await _context.Seats.FindAsync(id);
            if (seatInDb == null)
                return NotFound();

            // Możesz sprawdzić, czy zmieniana sala istnieje:
            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == dto.RoomId);
            if (!roomExists)
                return BadRequest($"Sala o ID {dto.RoomId} nie istnieje.");

            seatInDb.RoomId = dto.RoomId;
            seatInDb.Row = dto.Row;
            seatInDb.Column = dto.Column;

            _context.Entry(seatInDb).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ------------------------------------
        // 5) DELETE: api/seats/{id}
        //    -- dostępny tylko dla Admina
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var seatInDb = await _context.Seats.FindAsync(id);
            if (seatInDb == null)
                return NotFound();

            // Sprawdź, czy to miejsce zostało już zarezerwowane w przeszłości
            var isReserved = await _context.ReservationSeats
                .AnyAsync(rs => rs.SeatId == id);
            if (isReserved)
                return BadRequest("Nie można usunąć miejsca, ponieważ jest/zostało zarezerwowane.");

            _context.Seats.Remove(seatInDb);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
