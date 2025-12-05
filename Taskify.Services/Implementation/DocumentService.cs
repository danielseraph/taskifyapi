using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taskify.DataStore.Repositorise.Interface;
using Taskify.Domain.Entities;
using Taskify.Services.DTOs;
using Taskify.Services.Interface;

namespace Taskify.Services.Implementation
{
    public class DocumentService : IDocumentService
    {
        private readonly IAppRepository<Document> _documentRepository;
        private readonly IFileService _fileService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IProjectService _projectService;
        private IMapper _mapper;

        public DocumentService(
            IAppRepository<Document> documentRepository,
            IFileService fileService,
            ICurrentUserService currentUserService,
            IProjectService projectService,
            IMapper mapper)
        {
            _documentRepository = documentRepository;
            _fileService = fileService;
            _currentUserService = currentUserService;
            _projectService = projectService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DocumentDto>> GetAllAsync (Guid? projectId = null)
        {
            IQueryable<Document> query = _documentRepository.GetAllAsync(trackChanges: false);
            if (projectId.HasValue)
            {
                query = query.Where(d => d.ProjectId == projectId.Value);
            }
            var documents = await query.OrderByDescending(d => d.CreateAT).ToListAsync();
            List<DocumentDto> result = _mapper.Map<List<DocumentDto>>(documents);
            return result;
        }
        public async Task<ApiResponse<DocumentDto>> UploadAsync (IFormFile file, Guid projectId)
        {
            string? userId = _currentUserService.GetUserId();
            if(string.IsNullOrEmpty(userId))
            {
                return ApiResponseBuilder.Fail<DocumentDto>("user not authenticated", statusCode: StatusCodes.Status401Unauthorized);
            }
            var projectResult = await _projectService.GetProjectByIdAsync(projectId);
            if(!projectResult.IsSuccessful || projectResult.Data == null)
            {
                return ApiResponseBuilder.Fail<DocumentDto>("project not found", statusCode: StatusCodes.Status404NotFound);
            }
            if(file == null || file.Length == 0)
            {
                return ApiResponseBuilder.Fail<DocumentDto>("No file provided", statusCode: StatusCodes.Status400BadRequest);
            }

            //upload file to cloudinary via file service
            var uploadResult = await _fileService.UploadFileAsync(file, folder: "document");
            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
            {
                return ApiResponseBuilder.Fail<DocumentDto>("File upload failed", statusCode: StatusCodes.Status500InternalServerError);
            }
            Document document = new Document
            {
                Id = Guid.NewGuid(),
                OriginalFileName = file.FileName,
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty,
                ContentType = file.ContentType ?? "application/octet-stream",
                Size = file.Length,
                ProjectId = projectId,
                UploadedByUserId = userId,
                IsStarred = false,
                CreateAT = DateTime.UtcNow,
                UpdateAT = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document);
            bool saved = await _documentRepository.SaveChangesAsync();
            if (!saved)
            {
                // attempt to remove file from cloud to avoid orphaned storage (best-effort)
                try
                {
                    await _fileService.DeleteFileAsync(document.PublicId);
                }
                catch
                {
                    // ignore
                }

                return ApiResponseBuilder.Fail<DocumentDto>("Failed to persist document", statusCode: StatusCodes.Status500InternalServerError);
            }
            DocumentDto dto = new DocumentDto
            {
                Id = document.Id,
                FileName = document.OriginalFileName,
                PublicId = document.PublicId,
                Url = document.Url,
                ContentType = document.ContentType,
                Size = document.Size,
                ProjectId = document.ProjectId,
                UploadedBy = document.UploadedByUserId,
                IsStarred = document.IsStarred,
                CreateAt = document.CreateAT,
            };

            return ApiResponseBuilder.Success(dto, "File uploaded successfully", StatusCodes.Status201Created);
        }
        public async Task<ApiResponse<bool>> DeleteAsync (Guid documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if(document == null)
            {
                return ApiResponseBuilder.Fail<bool>("Document not found", statusCode: StatusCodes.Status404NotFound);
            }
            //delete from cloudinary via file service
            try
            {
                await _fileService.DeleteFileAsync(document.PublicId);
            }
            catch(Exception ex)
            {
                return ApiResponseBuilder.Fail<bool>($"Failed to delete file from storage: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
            }
            //delete from database
            await _documentRepository.DeleteAsync(document);
            bool saved = await _documentRepository.SaveChangesAsync();
            if(!saved)
            {
                return ApiResponseBuilder.Fail<bool>("Failed to delete document record", statusCode: StatusCodes.Status500InternalServerError);
            }
            return ApiResponseBuilder.Success(true, "Document deleted successfully", StatusCodes.Status200OK);
        }
        public async Task<DocumentDto?> GetByIdAsync (Guid documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if(document == null)
            {
                return null;
            }
            DocumentDto dto = _mapper.Map<DocumentDto>(document);
            return dto;
        }
        public async Task<ApiResponse<DocumentDto>> ToggleStarAsync(Guid id)
        {
            IQueryable<Document> tracked = _documentRepository.GetAllAsync(trackChanges: true);
            Document? document = await tracked.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                return ApiResponseBuilder.Fail<DocumentDto>("Document not found", statusCode: StatusCodes.Status404NotFound);
            }

            document.IsStarred = !document.IsStarred;
            await _documentRepository.UpdateAsync(document);
            bool saved = await _documentRepository.SaveChangesAsync();
            if (!saved)
            {
                return ApiResponseBuilder.Fail<DocumentDto>("Failed to update document", statusCode: StatusCodes.Status500InternalServerError);
            }

            DocumentDto dto = new DocumentDto
            {
                Id = document.Id,
                FileName = document.OriginalFileName,
                PublicId = document.PublicId!,
                Url = document.Url!,
                ContentType = document.ContentType,
                Size = document.Size,
                ProjectId = document.ProjectId,
                UploadedBy = document.UploadedByUserId,
                IsStarred = document.IsStarred,
                CreateAt = document.CreateAT,
            };

            return ApiResponseBuilder.Success(dto, "Document star toggled successfully");
        }
    }
}

