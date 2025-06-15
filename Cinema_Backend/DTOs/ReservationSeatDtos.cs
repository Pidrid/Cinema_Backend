namespace Cinema_Backend.DTOs
{
    // DTO zwracane przy GET (admin lub w kontekście rezerwacji)
    public class ReservationSeatDto
    {
        public int ReservationSeatId { get; set; }
        public int ReservationId { get; set; }
        public int SeatId { get; set; }
    }

    // DTO do utworzenia nowego powiązania (admin)
    public class ReservationSeatCreateDto
    {
        public int ReservationId { get; set; }
        public int SeatId { get; set; }
    }

    // DTO do aktualizacji (zmiana miejsca w ramach rezerwacji) – tylko admin
    public class ReservationSeatUpdateDto
    {
        public int ReservationId { get; set; }
        public int SeatId { get; set; }
    }
}
