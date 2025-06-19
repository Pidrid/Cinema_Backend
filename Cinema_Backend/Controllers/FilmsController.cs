using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cinema_Backend.Data;
using Cinema_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Cinema_Backend.DTOs;

namespace Cinema_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilmsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FilmsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //  GET: api/films
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FilmDto>>> GetAll()
        {
            var films = await _context.Films
                .Select(f => new FilmDto
                {
                    FilmId = f.FilmId,
                    Name = f.Name,
                    Description = f.Description
                })
                .ToListAsync();

            return Ok(films);
        }

        //  GET: api/films/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<FilmDto>> GetById(int id)
        {
            var film = await _context.Films
                .Where(f => f.FilmId == id)
                .Select(f => new FilmDto
                {
                    FilmId = f.FilmId,
                    Name = f.Name,
                    Description = f.Description
                })
                .FirstOrDefaultAsync();

            if (film == null)
                return NotFound();

            return Ok(film);
        }

        //  POST: api/films
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FilmDto>> Create([FromBody] FilmCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var film = new Film
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Films.Add(film);
            await _context.SaveChangesAsync();

            var resultDto = new FilmDto
            {
                FilmId = film.FilmId,
                Name = film.Name,
                Description = film.Description
            };

            return CreatedAtAction(nameof(GetById), new { id = film.FilmId }, resultDto);
        }

        //  PUT: api/films/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] FilmUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var filmInDb = await _context.Films.FindAsync(id);
            if (filmInDb == null)
                return NotFound();

            filmInDb.Name = dto.Name;
            filmInDb.Description = dto.Description;

            _context.Entry(filmInDb).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Error 204
        }

        //  DELETE: api/films/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var filmInDb = await _context.Films.FindAsync(id);
            if (filmInDb == null)
                return NotFound();

            _context.Films.Remove(filmInDb);
            await _context.SaveChangesAsync();

            return NoContent(); // Error 204
        }
    }
}
