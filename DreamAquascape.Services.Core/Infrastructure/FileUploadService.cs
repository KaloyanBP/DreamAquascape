using DreamAquascape.GCommon.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static DreamAquascape.GCommon.ApplicationConstants;

namespace DreamAquascape.Services.Core.Infrastructure
{
    public class FileUploadService : IFileUploadService
    {
        private readonly string _webRootPath;
        private readonly ILogger<FileUploadService> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly string[] _allowedImageExtensions;
        private readonly long _maxFileSize;
        private readonly string _uploadPath;

        public FileUploadService(
            string webRootPath,
            ILogger<FileUploadService> logger,
            IDateTimeProvider dateTimeProvider)
        {
            _webRootPath = webRootPath;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));

            _allowedImageExtensions = AllowedImageExtensions;
            _maxFileSize = MaxFileSize;
            _uploadPath = UploadPath;
        }

        public async Task<string> SaveEntryImageAsync(IFormFile file)
        {
            return await SaveFileAsync(file, EntryImageUploadPath);
        }

        public async Task<string> SaveContestImageAsync(IFormFile file)
        {
            return await SaveFileAsync(file, ContestImageUploadPath);
        }

        public async Task<string> SavePrizeImageAsync(IFormFile file)
        {
            return await SaveFileAsync(file, PrizeImageUploadPath);
        }

        public async Task<List<string>> SaveMultipleEntryImagesAsync(IFormFile[] files)
        {
            var imageUrls = new List<string>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var validationResult = ValidateImageFile(file);
                    if (!validationResult.IsValid)
                    {
                        errors.Add($"{file.FileName}: {validationResult.ErrorMessage}");
                        continue;
                    }

                    var imageUrl = await SaveEntryImageAsync(file);
                    imageUrls.Add(imageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error uploading file {file.FileName}");
                    errors.Add($"{file.FileName}: Upload failed");
                }
            }

            if (errors.Any())
            {
                string errorMessage = string.Join(", ", errors);
                _logger.LogWarning($"Some files failed to upload: {errorMessage}");
                throw new InvalidOperationException(errorMessage);
            }

            return imageUrls;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return false;

                var relativePath = imageUrl.TrimStart('/');
                var fullPath = Path.Combine(_webRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Image deleted successfully: {imageUrl}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image: {imageUrl}");
                return false;
            }
        }

        public FileValidationResult ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new FileValidationResult { IsValid = false, ErrorMessage = "No file selected." };
            }

            if (file.Length > _maxFileSize)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File size exceeds {GetMaxFileSizeInMb()}MB."
                };
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedImageExtensions.Contains(extension))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Only image files ({string.Join(", ", _allowedImageExtensions)}) are allowed."
                };
            }

            return new FileValidationResult { IsValid = true };
        }

        private async Task<string> SaveFileAsync(IFormFile file, string subfolder)
        {
            var timestamp = _dateTimeProvider.UtcNow.ToString("yyyyMMdd_HHmmss");
            var randomString = Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{timestamp}_{randomString}{extension}";

            var uploadDir = Path.Combine(_webRootPath, _uploadPath, subfolder);
            Directory.CreateDirectory(uploadDir);

            var filePath = Path.Combine(uploadDir, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{_uploadPath}/{subfolder}/{fileName}";
        }

        private int GetMaxFileSizeInMb()
        {
            return (int)(_maxFileSize / (1024 * 1024));
        }
    }
}
