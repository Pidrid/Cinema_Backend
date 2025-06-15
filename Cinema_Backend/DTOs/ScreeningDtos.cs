namespace Cinema_Backend.DTOs
{
    public class ScreeningDto
    {
        public int ScreeningId { get; set; }
        public int FilmId { get; set; }
        public string FilmName { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Price { get; set; }
        public string Language { get; set; }
        public string Subtitles { get; set; }
    }

    public class ScreeningCreateDto
    {
        public int FilmId { get; set; }
        public int RoomId { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Price { get; set; }
        public string Language { get; set; }
        public string Subtitles { get; set; }
    }

    public class ScreeningUpdateDto
    {
        public int FilmId { get; set; }
        public int RoomId { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Price { get; set; }
        public string Language { get; set; }
        public string Subtitles { get; set; }
    }
}
