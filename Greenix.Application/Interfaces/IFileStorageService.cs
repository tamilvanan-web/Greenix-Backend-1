using System.IO;
using System.Threading.Tasks;

namespace Greenix.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, string subFolder);
        void DeleteFile(string fileUrl);
    }
}
