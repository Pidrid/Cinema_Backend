namespace Cinema_Backend.Models
{
    public class Seat
    {
        public int SeatId { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public ICollection<ReservationSeat> ReservationSeats { get; set; }
    }
}
