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
    public class ScreeningsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ScreeningsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------------
        // 1) GET: api/screenings
        //    -- dostępne dla zalogowanych użytkowników
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ScreeningDto>>> GetAll()
        {
            var screenings = await _context.Screenings
                .Include(s => s.Film)
                .Include(s => s.Room)
                .Select(s => new ScreeningDto
                {
                    ScreeningId = s.ScreeningId,
                    FilmId = s.FilmId,
                    FilmName = s.Film.Name,
                    RoomId = s.RoomId,
                    RoomName = s.Room.Name,
                    DateTime = s.DateTime,
                    Price = s.Price,
                    Language = s.Language,
                    Subtitles = s.Subtitles
                })
                .ToListAsync();

            return Ok(screenings);
        }

        // ------------------------------------
        // 2) GET: api/screenings/{id}
        //    -- dostępne dla zalogowanych użytkowników
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<ScreeningDto>> GetById(int id)
        {
            var screening = await _context.Screenings
                .Include(s => s.Film)
                .Include(s => s.Room)
                .Where(s => s.ScreeningId == id)
                .Select(s => new ScreeningDto
                {
                    ScreeningId = s.ScreeningId,
                    FilmId = s.FilmId,
                    FilmName = s.Film.Name,
                    RoomId = s.RoomId,
                    RoomName = s.Room.Name,
                    DateTime = s.DateTime,
                    Price = s.Price,
                    Language = s.Language,
                    Subtitles = s.Subtitles
                })
                .FirstOrDefaultAsync();

            if (screening == null)
                return NotFound();

            return Ok(screening);
        }

        // ------------------------------------
        // 3) POST: api/screenings
        //    -- dostępny tylko dla Admina
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ScreeningDto>> Create([FromBody] ScreeningCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Sprawdź, czy film i sala istnieją
            var filmExists = await _context.Films.AnyAsync(f => f.FilmId == dto.FilmId);
            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == dto.RoomId);
            if (!filmExists)
                return BadRequest($"Film o ID {dto.FilmId} nie istnieje.");
            if (!roomExists)
                return BadRequest($"Sala o ID {dto.RoomId} nie istnieje.");

            var screening = new Screening
            {
                FilmId = dto.FilmId,
                RoomId = dto.RoomId,
                DateTime = dto.DateTime,
                Price = dto.Price,
                Language = dto.Language,
                Subtitles = dto.Subtitles
            };

            _context.Screenings.Add(screening);
            await _context.SaveChangesAsync();

            // Ponownie pobieramy nazwę filmu i sali dla DTO
            var created = await _context.Screenings
                .Include(s => s.Film)
                .Include(s => s.Room)
                .Where(s => s.ScreeningId == screening.ScreeningId)
                .Select(s => new ScreeningDto
                {
                    ScreeningId = s.ScreeningId,
                    FilmId = s.FilmId,
                    FilmName = s.Film.Name,
                    RoomId = s.RoomId,
                    RoomName = s.Room.Name,
                    DateTime = s.DateTime,
                    Price = s.Price,
                    Language = s.Language,
                    Subtitles = s.Subtitles
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetById), new { id = created.ScreeningId }, created);
        }

        // ------------------------------------
        // 4) PUT: api/screenings/{id}
        //    -- dostępny tylko dla Admina
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ScreeningUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var screeningInDb = await _context.Screenings.FindAsync(id);
            if (screeningInDb == null)
                return NotFound();

            // Sprawdź, czy film i sala istnieją
            var filmExists = await _context.Films.AnyAsync(f => f.FilmId == dto.FilmId);
            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == dto.RoomId);
            if (!filmExists)
                return BadRequest($"Film o ID {dto.FilmId} nie istnieje.");
            if (!roomExists)
                return BadRequest($"Sala o ID {dto.RoomId} nie istnieje.");

            screeningInDb.FilmId = dto.FilmId;
            screeningInDb.RoomId = dto.RoomId;
            screeningInDb.DateTime = dto.DateTime;
            screeningInDb.Price = dto.Price;
            screeningInDb.Language = dto.Language;
            screeningInDb.Subtitles = dto.Subtitles;

            _context.Entry(screeningInDb).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ------------------------------------
        // 5) DELETE: api/screenings/{id}
        //    -- dostępny tylko dla Admina
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var screeningInDb = await _context.Screenings.FindAsync(id);
            if (screeningInDb == null)
                return NotFound();

            // Sprawdź, czy nie ma rezerwacji do tego seansu
            var hasReservations = await _context.Reservations
                .AnyAsync(r => r.ScreeningId == id);
            if (hasReservations)
                return BadRequest("Nie można usunąć seansu, ponieważ są powiązane rezerwacje.");

            _context.Screenings.Remove(screeningInDb);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
