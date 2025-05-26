namespace Cinema_Backend.Models
{
    public class Screening
    {
        public int ScreeningId { get; set; }
        public int FilmId { get; set; }
        public Film Film { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Price { get; set; }
        public string Language { get; set; }
        public string Subtitles { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
