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

        // ------------------------------------
        // 1) GET: api/reservationseats
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

        // ------------------------------------
        // 2) GET: api/reservationseats/reservation/{reservationId}
        //      Admin: zwraca dowolne
        //      Użytkownik: tylko te, które należą do jego rezerwacji
        [HttpGet("reservation/{reservationId:int}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReservationSeatDto>>> GetByReservation(int reservationId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
                return Forbid();

            // Pobranie rezerwacji z bazy aby sprawdzić właściciela
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
        // 3) GET: api/reservationseats/{id}
        //    jeśli Admin: zwraca dowolne
        //    jeśli użytkownik: musi być właścicielem powiązanej rezerwacji
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

        // ------------------------------------
        // 4) POST: api/reservationseats
        //    tylko Admin może tworzyć "oddzieln"e wiersze ReservationSeat.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ReservationSeatDto>> Create([FromBody] ReservationSeatCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Sprawdź istnieje rezerwacja
            var reservationExists = await _context.Reservations
                .AnyAsync(r => r.ReservationId == dto.ReservationId);
            if (!reservationExists)
                return BadRequest($"Rezerwacja o ID {dto.ReservationId} nie istnieje.");

            // Sprawdź istnieje miejsce
            var seatExists = await _context.Seats
                .AnyAsync(s => s.SeatId == dto.SeatId);
            if (!seatExists)
                return BadRequest($"Miejsce o ID {dto.SeatId} nie istnieje.");

            // Sprawdź, czy dana relacja nie istnieje już (unikalność pola ReservationId+SeatId)
            var duplicate = await _context.ReservationSeats
                .AnyAsync(rs => rs.ReservationId == dto.ReservationId && rs.SeatId == dto.SeatId);
            if (duplicate)
                return BadRequest("Ta para (ReservationId + SeatId) już istnieje.");

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

        // ------------------------------------
        // 5) PUT: api/reservationseats/{id}
        //    -- tylko Admin może edytować
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ReservationSeatUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var rsInDb = await _context.ReservationSeats.FindAsync(id);
            if (rsInDb == null)
                return NotFound();

            // Sprawdź istnienie rezerwacji i miejsca (jak wyżej)
            var reservationExists = await _context.Reservations
                .AnyAsync(r => r.ReservationId == dto.ReservationId);
            if (!reservationExists)
                return BadRequest($"Rezerwacja o ID {dto.ReservationId} nie istnieje.");

            var seatExists = await _context.Seats
                .AnyAsync(s => s.SeatId == dto.SeatId);
            if (!seatExists)
                return BadRequest($"Miejsce o ID {dto.SeatId} nie istnieje.");

            // Upewnij się, że nie wprowadzasz duplikatu (ReservationId+SeatId)
            var duplicate = await _context.ReservationSeats
                .AnyAsync(x => x.ReservationId == dto.ReservationId
                               && x.SeatId == dto.SeatId
                               && x.ReservationSeatId != id);
            if (duplicate)
                return BadRequest("Ta para (ReservationId + SeatId) już istnieje w innej krotce.");

            // Nadpisujemy:
            rsInDb.ReservationId = dto.ReservationId;
            rsInDb.SeatId = dto.SeatId;

            _context.Entry(rsInDb).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ------------------------------------
        // 6) DELETE: api/reservationseats/{id}
        //    -- tylko Admin może usuwać
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
