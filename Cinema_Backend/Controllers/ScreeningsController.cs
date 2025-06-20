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

        //  GET: api/screenings
        [HttpGet]
        [AllowAnonymous]
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

        //  GET: api/screenings/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
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

        // GET: api/screenings/byday?date=2025-06-20
        [HttpGet("byday")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ScreeningDto>>> GetScreeningsByDay(DateTime date)
        {
            var nextDay = date.Date.AddDays(1);

            var screenings = await _context.Screenings
                .Include(s => s.Film)
                .Include(s => s.Room)
                .Where(s => s.DateTime >= date.Date && s.DateTime < nextDay)
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

        //  POST: api/screenings
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ScreeningDto>> Create([FromBody] ScreeningCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var filmExists = await _context.Films.AnyAsync(f => f.FilmId == dto.FilmId);
            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == dto.RoomId);
            if (!filmExists)
                return BadRequest($"Film with ID {dto.FilmId} doesn't exist.");
            if (!roomExists)
                return BadRequest($"Room with ID {dto.RoomId} doesn't exist.");

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

            // Load the created screening with related entities
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

        //  PUT: api/screenings/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ScreeningUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var screeningInDb = await _context.Screenings.FindAsync(id);
            if (screeningInDb == null)
                return NotFound();

            var filmExists = await _context.Films.AnyAsync(f => f.FilmId == dto.FilmId);
            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == dto.RoomId);
            if (!filmExists)
                return BadRequest($"Film with ID {dto.FilmId} doesn't exist.");
            if (!roomExists)
                return BadRequest($"Room with ID {dto.RoomId} doesn't exist.");

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

        //  DELETE: api/screenings/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var screeningInDb = await _context.Screenings.FindAsync(id);
            if (screeningInDb == null)
                return NotFound();

            var hasReservations = await _context.Reservations
                .AnyAsync(r => r.ScreeningId == id);
            if (hasReservations)
                return BadRequest("You can't delete screening with associated reservations");

            _context.Screenings.Remove(screeningInDb);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
