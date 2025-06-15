namespace Cinema_Backend.DTOs
{
    public class SeatDto
    {
        public int SeatId { get; set; }
        public int RoomId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public class SeatCreateDto
    {
        public int RoomId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public class SeatUpdateDto
    {
        public int RoomId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }
}
