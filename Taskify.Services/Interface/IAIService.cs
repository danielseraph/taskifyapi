using Taskify.Services.DTOs;

namespace Taskify.Services.Interface
{
    public interface IAIService
    {
        /// <summary>
        /// Send a prompt to the configured AI backend and return the text response.
        /// </summary>
        Task<string> ProcessChatAsync(ChatRequest request, CancellationToken cancellationToken = default);
    }
}