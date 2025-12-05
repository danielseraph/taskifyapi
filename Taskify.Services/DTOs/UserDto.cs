namespace Taskify.Services.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = default!;
        public string profileImage { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
