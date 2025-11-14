using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace HealthBookingSystem.Service
{
    public class PhotoService
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public PhotoService(CloudinaryDotNet.Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file.Length <= 0) return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream)
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();  // Trả về URL của ảnh
        }
    }

}
