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
    public class ReservationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //  GET: api/reservations
        //  If admin: return all reservations
        //  If user: return only their reservations
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAll()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
                return Forbid();

            if (User.IsInRole("Admin"))
            {
                var allReservations = await _context.Reservations
                    .Include(r => r.ReservationSeats)
                        .ThenInclude(rs => rs.Seat)
                    .Select(r => new ReservationDto
                    {
                        ReservationId = r.ReservationId,
                        ScreeningId = r.ScreeningId,
                        DateTime = r.DateTime,
                        Subtotal = r.Subtotal,
                        Discount = r.Discount,
                        Tax = r.Tax,
                        Total = r.Total,
                        Seats = r.ReservationSeats.Select(rs => new SeatDto
                        {
                            SeatId = rs.Seat.SeatId,
                            RoomId = rs.Seat.RoomId,
                            Row = rs.Seat.Row,
                            Column = rs.Seat.Column
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(allReservations);
            }
            else
            {
                var userReservations = await _context.Reservations
                    .Where(r => r.UserId == userIdString)
                    .Include(r => r.ReservationSeats)
                        .ThenInclude(rs => rs.Seat)
                    .Select(r => new ReservationDto
                    {
                        ReservationId = r.ReservationId,
                        ScreeningId = r.ScreeningId,
                        DateTime = r.DateTime,
                        Subtotal = r.Subtotal,
                        Discount = r.Discount,
                        Tax = r.Tax,
                        Total = r.Total,
                        Seats = r.ReservationSeats.Select(rs => new SeatDto
                        {
                            SeatId = rs.Seat.SeatId,
                            RoomId = rs.Seat.RoomId,
                            Row = rs.Seat.Row,
                            Column = rs.Seat.Column
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(userReservations);
            }
        }

        //  GET: api/reservations/{id}
        //  Admin or user who made the reservation can access it
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<ReservationDto>> GetById(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
                return Forbid();

            var reservation = await _context.Reservations
                .Include(r => r.ReservationSeats)
                    .ThenInclude(rs => rs.Seat)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            if (!User.IsInRole("Admin") && reservation.UserId != userIdString)
                return Forbid();

            var dto = new ReservationDto
            {
                ReservationId = reservation.ReservationId,
                ScreeningId = reservation.ScreeningId,
                DateTime = reservation.DateTime,
                Subtotal = reservation.Subtotal,
                Discount = reservation.Discount,
                Tax = reservation.Tax,
                Total = reservation.Total,
                Seats = reservation.ReservationSeats.Select(rs => new SeatDto
                {
                    SeatId = rs.Seat.SeatId,
                    RoomId = rs.Seat.RoomId,
                    Row = rs.Seat.Row,
                    Column = rs.Seat.Column
                }).ToList()
            };

            return Ok(dto);
        }

        //  POST: api/reservations
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReservationDto>> Create([FromBody] ReservationCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Taken screening by ID
            var screening = await _context.Screenings
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.ScreeningId == dto.ScreeningId);

            if (screening == null)
                return BadRequest($"Screening with ID {dto.ScreeningId} doesn't exist.");

            // Taken seats by IDs
            var seats = await _context.Seats
                .Where(s => dto.SeatIds.Contains(s.SeatId) && s.RoomId == screening.RoomId)
                .ToListAsync();

            if (seats.Count != dto.SeatIds.Count)
                return BadRequest("At least one of these seats doesn't exist or belongs to other room.");

            // Check if any of the selected seats are already booked for this screening
            var alreadyBookedSeats = await _context.ReservationSeats
                .Include(rs => rs.Reservation)
                .Where(rs => dto.SeatIds.Contains(rs.SeatId)
                             && rs.Reservation.ScreeningId == dto.ScreeningId)
                .Select(rs => rs.SeatId)
                .ToListAsync();

            if (alreadyBookedSeats.Any())
                return BadRequest($"Seats with ID {string.Join(", ", alreadyBookedSeats)} are already taken.");

            var subtotal = screening.Price * dto.SeatIds.Count;
            decimal discount = 0;
            decimal tax = Math.Round(subtotal * 0.08m, 2);
            var total = subtotal - discount;

            var reservation = new Reservation
            {
                ScreeningId = dto.ScreeningId,
                UserId = userIdString,
                Subtotal = subtotal,
                Discount = discount,
                Tax = tax,
                Total = total,
                DateTime = DateTime.UtcNow,
                ReservationSeats = dto.SeatIds
                    .Select(id => new ReservationSeat { SeatId = id })
                    .ToList()
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var resultDto = new ReservationDto
            {
                ReservationId = reservation.ReservationId,
                ScreeningId = reservation.ScreeningId,
                DateTime = reservation.DateTime,
                Subtotal = reservation.Subtotal,
                Discount = reservation.Discount,
                Tax = reservation.Tax,
                Total = reservation.Total,
                Seats = seats.Select(s => new SeatDto
                {
                    SeatId = s.SeatId,
                    RoomId = s.RoomId,
                    Row = s.Row,
                    Column = s.Column
                }).ToList()
            };

            return CreatedAtAction(nameof(GetById), new { id = reservation.ReservationId }, resultDto);
        }

        //  DELETE: api/reservations/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var reservationInDb = await _context.Reservations
                .Include(r => r.ReservationSeats)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservationInDb == null)
                return NotFound();

            _context.ReservationSeats.RemoveRange(reservationInDb.ReservationSeats);
            _context.Reservations.Remove(reservationInDb);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
