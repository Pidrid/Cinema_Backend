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
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //  GET: api/rooms
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAll()
        {
            var rooms = await _context.Rooms
                .Select(r => new RoomDto
                {
                    RoomId = r.RoomId,
                    Name = r.Name
                })
                .ToListAsync();

            return Ok(rooms);
        }

        // GET: api/rooms/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<RoomDto>> GetById(int id)
        {
            var room = await _context.Rooms
                .Where(r => r.RoomId == id)
                .Select(r => new RoomDto
                {
                    RoomId = r.RoomId,
                    Name = r.Name
                })
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            return Ok(room);
        }

        //  POST: api/rooms
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoomDto>> Create([FromBody] RoomCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var room = new Room
            {
                Name = dto.Name
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var resultDto = new RoomDto
            {
                RoomId = room.RoomId,
                Name = room.Name
            };

            return CreatedAtAction(nameof(GetById), new { id = room.RoomId }, resultDto);
        }

        //  PUT: api/rooms/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] RoomUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roomInDb = await _context.Rooms.FindAsync(id);
            if (roomInDb == null)
                return NotFound();

            roomInDb.Name = dto.Name;
            _context.Entry(roomInDb).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //  DELETE: api/rooms/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var roomInDb = await _context.Rooms.FindAsync(id);
            if (roomInDb == null)
                return NotFound();

            var hasSeats = await _context.Seats.AnyAsync(s => s.RoomId == id);
            var hasScreenings = await _context.Screenings.AnyAsync(s => s.RoomId == id);
            if (hasSeats || hasScreenings)
                return BadRequest("You can't delete room with associated seats or screenings.");

            _context.Rooms.Remove(roomInDb);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
