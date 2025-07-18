using Microsoft.AspNetCore.Http;

namespace DreamAquascape.Services.Core.Interfaces
{
    public class FileValidationResult
    {       
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public interface IFileUploadService
    {
        Task<string> SaveEntryImageAsync(IFormFile file);
        Task<string> SaveContestImageAsync(IFormFile file);
        Task<string> SavePrizeImageAsync(IFormFile file);
        Task<List<string>> SaveMultipleEntryImagesAsync(IFormFile[] files);
        Task<bool> DeleteImageAsync(string imageUrl);
        FileValidationResult ValidateImageFile(IFormFile file);
    }
}
