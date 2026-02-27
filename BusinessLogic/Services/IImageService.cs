using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string imageUrl);
    }
}
