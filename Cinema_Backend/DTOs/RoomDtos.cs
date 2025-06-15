namespace Cinema_Backend.DTOs
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string Name { get; set; }
    }

    public class RoomCreateDto
    {
        public string Name { get; set; }
    }

    public class RoomUpdateDto
    {
        public string Name { get; set; }
    }
}
