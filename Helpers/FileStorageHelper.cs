using SixLabors.ImageSharp.Formats;
using System.Security.Principal;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Collections.Generic;

namespace Baluma.Emblue.ApiConsumer.App.Helpers
{
    public static class FileStorageHelper
    {

        private static readonly HashSet<string> AllowedImageFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "jpeg", "png", "jpg", "gif", "webp"
        };

        private static readonly HashSet<string> AllowedVideoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4"
        };

        public static bool IsSupportedVideoFormat(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName);
            return AllowedVideoExtensions.Contains(extension);
        }

        public static bool IsSupportedImageFormat(byte[] fileBytes)
        {
            try
            {
                using var stream = new MemoryStream(fileBytes);

                // Usa Image.DetectFormat para obtener directamente el formato sin necesidad de cargar la imagen
                var format = Image.DetectFormat(stream);

                return format != null && AllowedImageFormats.Contains(format.Name.ToUpperInvariant());
            }
            catch
            {
                return false;
            }
        }

        public static bool IsSupportedImageFormatFile(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext);
        }



        public static async Task<bool> SaveFileWithImpersonationAsync(IConfiguration configuration, string fullFilePath, byte[] fileBytes)
        {
            if (string.IsNullOrWhiteSpace(fullFilePath))
                throw new ArgumentException("El parámetro 'fullFilePath' no puede ser nulo o vacío.");

            if (fileBytes == null || fileBytes.Length == 0)
                throw new ArgumentException("El archivo está vacío o es nulo.");

            string normalizedPath = Path.GetFullPath(fullFilePath);

            using var impersonationHelper = new ImpersonationHelper();
            WindowsIdentity? identity = null;
            bool isImpersonated = false;

            try
            {
                var filesConnection = configuration.GetSection("FilesConnection");
                isImpersonated = impersonationHelper.ImpersonateUser(
                    filesConnection["User"],
                    filesConnection["Domain"],
                    filesConnection["Password"],
                    out identity);

                if (!isImpersonated || identity == null)
                    return false;

                return await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                {
                    var directory = Path.GetDirectoryName(normalizedPath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory!);

                    await File.WriteAllBytesAsync(normalizedPath, fileBytes);
                    return true;
                });
            }
            catch (Exception ex)
            {
                // Ideal: usar ILogger, aquí dejamos el fallback
                Console.Error.WriteLine($"[FileStorageHelper] Error guardando archivo en '{fullFilePath}': {ex.Message}");
                return false;
            }
            finally
            {
                identity?.Dispose();
            }
        }
    }

}
