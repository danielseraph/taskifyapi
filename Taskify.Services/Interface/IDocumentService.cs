using Microsoft.AspNetCore.Http;
using Taskify.Services.DTOs;

namespace Taskify.Services.Interface
{
    public interface IDocumentService
    {
        Task<IEnumerable<DocumentDto>> GetAllAsync(Guid? projectId = null);
        Task<ApiResponse<DocumentDto>> ToggleStarAsync(Guid id);
        Task<DocumentDto?> GetByIdAsync(Guid documentId);
        Task<ApiResponse<bool>> DeleteAsync(Guid documentId);
        Task<ApiResponse<DocumentDto>> UploadAsync(IFormFile file, Guid projectId);
    }
}