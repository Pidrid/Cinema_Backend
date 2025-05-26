using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cinema_Backend.Data;
using Cinema_Backend.Models;

namespace Cinema_Backend.Controllers
{
    public class ReservationSeatsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationSeatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ReservationSeats
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ReservationSeats.Include(r => r.Reservation).Include(r => r.Seat);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ReservationSeats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationSeat = await _context.ReservationSeats
                .Include(r => r.Reservation)
                .Include(r => r.Seat)
                .FirstOrDefaultAsync(m => m.ReservationSeatId == id);
            if (reservationSeat == null)
            {
                return NotFound();
            }

            return View(reservationSeat);
        }

        // GET: ReservationSeats/Create
        public IActionResult Create()
        {
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "ReservationId", "ReservationId");
            ViewData["SeatId"] = new SelectList(_context.Seats, "SeatId", "SeatId");
            return View();
        }

        // POST: ReservationSeats/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReservationSeatId,ReservationId,SeatId")] ReservationSeat reservationSeat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reservationSeat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "ReservationId", "ReservationId", reservationSeat.ReservationId);
            ViewData["SeatId"] = new SelectList(_context.Seats, "SeatId", "SeatId", reservationSeat.SeatId);
            return View(reservationSeat);
        }

        // GET: ReservationSeats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationSeat = await _context.ReservationSeats.FindAsync(id);
            if (reservationSeat == null)
            {
                return NotFound();
            }
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "ReservationId", "ReservationId", reservationSeat.ReservationId);
            ViewData["SeatId"] = new SelectList(_context.Seats, "SeatId", "SeatId", reservationSeat.SeatId);
            return View(reservationSeat);
        }

        // POST: ReservationSeats/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationSeatId,ReservationId,SeatId")] ReservationSeat reservationSeat)
        {
            if (id != reservationSeat.ReservationSeatId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservationSeat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationSeatExists(reservationSeat.ReservationSeatId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "ReservationId", "ReservationId", reservationSeat.ReservationId);
            ViewData["SeatId"] = new SelectList(_context.Seats, "SeatId", "SeatId", reservationSeat.SeatId);
            return View(reservationSeat);
        }

        // GET: ReservationSeats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationSeat = await _context.ReservationSeats
                .Include(r => r.Reservation)
                .Include(r => r.Seat)
                .FirstOrDefaultAsync(m => m.ReservationSeatId == id);
            if (reservationSeat == null)
            {
                return NotFound();
            }

            return View(reservationSeat);
        }

        // POST: ReservationSeats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservationSeat = await _context.ReservationSeats.FindAsync(id);
            if (reservationSeat != null)
            {
                _context.ReservationSeats.Remove(reservationSeat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationSeatExists(int id)
        {
            return _context.ReservationSeats.Any(e => e.ReservationSeatId == id);
        }
    }
}
