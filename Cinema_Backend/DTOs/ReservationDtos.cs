namespace Cinema_Backend.DTOs
{
    public class ReservationDto
    {
        public int ReservationId { get; set; }
        public int ScreeningId { get; set; }
        public DateTime DateTime { get; set; }      // kiedy utworzono rezerwację
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public List<SeatDto> Seats { get; set; }
    }

    // DTO do wywołania tworzenia rezerwacji (user)
    public class ReservationCreateDto
    {
        public int ScreeningId { get; set; }
        public List<int> SeatIds { get; set; }    
    }

    // DTO do zwracania szczegółów (Admin)
    public class ReservationAdminDto
    {
        public int ReservationId { get; set; }
        public int ScreeningId { get; set; }
        public string FilmName { get; set; }
        public string RoomName { get; set; }
        public DateTime ScreeningDateTime { get; set; }
        public string UserEmail { get; set; }
        public DateTime ReservationDateTime { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public List<SeatDto> Seats { get; set; }
    }
}
