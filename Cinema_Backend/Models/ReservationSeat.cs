namespace Cinema_Backend.Models
{
    public class ReservationSeat
    {
        public int ReservationSeatId { get; set; }
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        public int SeatId { get; set; }
        public Seat Seat { get; set; }
    }
}
