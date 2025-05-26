namespace Cinema_Backend.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string Name { get; set; }
        public ICollection<Seat> Seats { get; set; }
        public ICollection<Screening> Screenings { get; set; }
    }
}
