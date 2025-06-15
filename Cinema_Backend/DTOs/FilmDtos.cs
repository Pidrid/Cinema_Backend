namespace Cinema_Backend.DTOs
{
    public class FilmDto
    {
        public int FilmId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class FilmCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class FilmUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
