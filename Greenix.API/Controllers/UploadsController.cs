using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Greenix.Shared.Models;
using Greenix.Application.Interfaces;
using Greenix.Application.DTOs.Upload;

namespace Greenix.API.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/uploads")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        
        // Define strict verification bounds
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB limit

        public UploadsController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<UploadResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<UploadResponse>))]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string subFolder = "complaints")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new BaseResponse<UploadResponse>("No file was submitted."));
            }

            // 1. Validate File Size
            if (file.Length > MaxFileSizeInBytes)
            {
                return BadRequest(new BaseResponse<UploadResponse>("File size exceeds the maximum limit of 5MB."));
            }

            // 2. Validate Extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(new BaseResponse<UploadResponse>("Unsupported file format. Only JPEG, JPG, and PNG images are allowed."));
            }

            // 3. Clean subfolder routing input
            var sanitizedSubfolder = string.IsNullOrWhiteSpace(subFolder) 
                ? "general" 
                : Path.GetFileName(subFolder); // Strips directory traversals like "../../"

            try
            {
                using var stream = file.OpenReadStream();
                var relativeUrl = await _fileStorageService.SaveFileAsync(
                    stream, 
                    file.FileName, 
                    file.ContentType, 
                    sanitizedSubfolder
                );

                var responseData = new UploadResponse
                {
                    Url = relativeUrl,
                    FileName = Path.GetFileName(relativeUrl),
                    Size = file.Length
                };

                return Ok(new BaseResponse<UploadResponse>(responseData, "File uploaded successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new BaseResponse<UploadResponse>($"An error occurred during file upload: {ex.Message}"));
            }
        }
    }
}
