using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskify.Services.Interface;

namespace Taskify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllDocuments(Guid? projectId = null)
        {
            var documents = await _documentService.GetAllAsync(projectId);
            return Ok(documents);
        }
        [HttpGet]
        [Route("{documentId:guid}")]
        public async Task<IActionResult> GetDocumentById(Guid documentId)
        {
            var document = await _documentService.GetByIdAsync(documentId);
            if (document == null)
            {
                return NotFound();
            }
            return Ok(document);
        }
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadDocument(IFormFile file, Guid projectId)
        {
            var result = await _documentService.UploadAsync(file, projectId);
            if (!result.IsSuccessful)
            {
                return StatusCode(result.StatusCode, result.Message);
            }
            return Ok(result.Data);
        }
        [HttpDelete]
        [Route("{documentId:guid}")]
        public async Task<IActionResult> DeleteDocument( Guid documentId)
        {
            var result = await _documentService.DeleteAsync(documentId);
            if (!result.IsSuccessful)
            {
                return StatusCode(result.StatusCode, result.Message);
            }
            return Ok(new { success = true });
        }
        [HttpGet]
        [Route("{documentId:guid}/toggle-star")]
        public async Task<IActionResult> ToggleStarDocument( Guid documentId)
        {
            var result = await _documentService.ToggleStarAsync(documentId);
            if (!result.IsSuccessful)
            {
                return StatusCode(result.StatusCode, result.Message);
            }
            return Ok(result.Data);
        }
    }
}
