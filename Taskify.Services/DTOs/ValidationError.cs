namespace Taskify.Services.DTOs
{
    public class ValidationError
    {
        public string? Property { get; set; } 
        public IEnumerable<string> Error {  get; set; } = Enumerable.Empty<string>();
    }
}
