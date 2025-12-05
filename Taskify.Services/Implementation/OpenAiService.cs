using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.ClientModel;
using Taskify.Services.DTOs;
using Taskify.Services.Interface;
using Taskify.Services.Utilities;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Taskify.Services.Implementation
{
    public class OpenAiService : IAIService
    {
        private readonly IChatClient _client;

        public OpenAiService(IOptions<AiConfig> config)
        {
            var cfg = config.Value;

            _client = new ChatClient(
                cfg.Model,
                new ApiKeyCredential(cfg.ApiKey),
                new OpenAI.OpenAIClientOptions
                {
                    Endpoint = new Uri(cfg.BaseUrl)
                }).AsIChatClient();
        }


        public async Task<string> ProcessChatAsync(ChatRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                throw new ArgumentException("Message cannot be empty.");

            var history = new List<ChatMessage>();

            if (request.History != null)
            {
                foreach (var msg in request.History)
                {
                    history.Add(new ChatMessage(ChatRole.User, msg));
                }
            }

            history.Add(new ChatMessage(ChatRole.User, request.Message));

            string aiReply = string.Empty;

            await foreach (var token in _client.GetStreamingResponseAsync(history))
            {
                aiReply += token.Text;
            }
            history.Add(new ChatMessage(ChatRole.Assistant, aiReply));

            return aiReply;
        }
    }
}
