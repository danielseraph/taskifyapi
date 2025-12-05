using Microsoft.AspNetCore.Mvc;
using Taskify.Services.DTOs;
using Taskify.Services.Implementation;
using Taskify.Services.Interface;

namespace Taskify.Api.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IAIService _ai;

        public AiController(IAIService ai)
        {
            _ai = ai;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var result = await _ai.ProcessChatAsync(request);

            return Ok(new { message = result });
        }
    }
}