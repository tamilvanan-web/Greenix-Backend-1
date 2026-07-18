using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Greenix.Application.Interfaces;

namespace Greenix.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LocalFileStorageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, string subFolder)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            // Sanitize file name by generating a unique GUID to prevent directory traversal and name clashes
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Ensure directory exists in wwwroot
            var baseDirectory = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var targetDirectory = Path.Combine(baseDirectory, "uploads", subFolder);

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            var filePath = Path.Combine(targetDirectory, uniqueFileName);

            // Save stream to physical file path
            using (var destinationStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await fileStream.CopyToAsync(destinationStream);
            }

            // Return relative serving URL path
            return $"/uploads/{subFolder}/{uniqueFileName}";
        }

        public void DeleteFile(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            // Resolve file system path from relative URL
            var relativePath = fileUrl.TrimStart('/');
            var baseDirectory = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(baseDirectory, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
