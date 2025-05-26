namespace Cinema_Backend.Models
{
    public class Film
    {
        public int FilmId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Screening> Screenings { get; set; }
    }
}
