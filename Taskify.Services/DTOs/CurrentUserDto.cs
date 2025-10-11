namespace Taskify.Services.DTOs
{
    public class CurrentUserDto
    {

        public string Id { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public IList<string> Role { get; set; } = new List<string>();

    }
}
