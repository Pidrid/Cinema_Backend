namespace Cinema_Backend.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }

        public int ScreeningId { get; set; }
        public Screening Screening { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public DateTime DateTime { get; set; }

        public ICollection<ReservationSeat> ReservationSeats { get; set; }
    }
}
