using Microsoft.AspNetCore.Http;

namespace DentalNUB.Interface
{
    public interface IImageService
    {
        Task<string> UploadAsync(IFormFile file, string folder);
    }
}