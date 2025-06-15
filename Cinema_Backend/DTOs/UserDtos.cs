namespace Cinema_Backend.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }       // (GUID) generowany przez Identity
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
