
using System.Security.Principal;
using NewsService.Baluma.Marketing.api.Dtos;
using NewsService.Baluma.Marketing.api.Models;

using SixLabors.ImageSharp; // Importar ImageSharp
using SixLabors.ImageSharp.Processing; // Procesamiento de imagen
using SixLabors.ImageSharp.Formats; // Soporte de formatos
using SixLabors.ImageSharp.PixelFormats; // Pixel format
using SixLabors.ImageSharp.IO; // IO support
using ScheduleService.Baluma.RRHH.api.Helpers;
using System.Configuration;
using System.Collections.Concurrent;
using System.Drawing.Imaging;

using DrawingImage = System.Drawing.Image;
using NewsService.Common;

namespace Baluma.Emblue.ApiConsumer.App.Helpers
{

    public class FilesHelper
    {

        public static async Task<string?> GetImageBase64Async(IConfiguration configuration, string imagePath)
        {
            using (var impersonationHelper = new ImpersonationHelper())
            {
                WindowsIdentity? identity = null;
                bool isImpersonated = false;

                try
                {

                    string? base64Image = null;

                    var filesConnection = configuration.GetSection("FilesConnection");
                    // Impersonar al usuario antes de intentar acceder al archivo
                    isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {

                            if (!File.Exists(imagePath))
                            {
                                base64Image = null; // Si la imagen no existe, retornar null
                            }
                            else
                            {

                                // Leer la imagen como bytes
                                var imageBytes = await File.ReadAllBytesAsync(imagePath);

                                base64Image = Convert.ToBase64String(imageBytes);
                            }
                        });

                        // Convertir la imagen en base64
                        return base64Image;
                    }
                    else
                    {
                        return null; // Si la suplantación falla
                    }
                }
                finally
                {
                    // Asegúrate de restablecer la identidad
                    identity?.Dispose();
                }
            }
        }



        // public static async Task<List<string>> GetNewsBase64Async(NewsDto newItem, IConfiguration configuration)
        public static async Task<string?> GetNewsBase64Async(NewsDto newItem, IConfiguration configuration)
        {
            // var base64Images = new List<string>();

            using (var impersonationHelper = new ImpersonationHelper())
            {
                WindowsIdentity? identity = null;
                bool isImpersonated = false;

                try
                {

                    string? base64Image = null;

                    var filesConnection = configuration.GetSection("FilesConnection");
                    // Impersonar al usuario antes de intentar acceder al archivo
                    isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {

                            // Obtener la ruta del archivo desde la entidad (puede ser MessageFilePath o similar)
                            var filePath = Path.Combine(newItem.ImagePath, newItem.ImageName);

                            if (!File.Exists(filePath))
                            {
                                base64Image = null;
                                //throw new FileNotFoundException($"Image not found at path: {filePath}");
                            }
                            else { 

                                // Leer la imagen como bytes
                                var imageBytes = await File.ReadAllBytesAsync(filePath);

                                // Convertir la imagen en base64
                                base64Image = Convert.ToBase64String(imageBytes);
                            }

                        });

                        return base64Image;

                        // Agregar la imagen en base64 a la lista
                        //     base64Images.Add(base64Image);

                        // return base64Images;
                    }
                    else
                    {
                        return null; // Si la suplantación falla
                    }
                }
                finally
                {
                    // Asegúrate de restablecer la identidad
                    identity?.Dispose();
                }
            }

            
        }

        public static async Task<List<object?>> GetNewsListBase64Async(List<NewsDto> newsItems, IConfiguration configuration)
        {
            // var base64Images = new List<string>();

            using (var impersonationHelper = new ImpersonationHelper())
            {
                WindowsIdentity? identity = null;
                bool isImpersonated = false;

                try
                {

                    string? base64Image = null;

                    var filesConnection = configuration.GetSection("FilesConnection");
                    // Impersonar al usuario antes de intentar acceder al archivo
                    isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        var newsItemsWithImages = new List<object>();

                        await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {

                            foreach (var newsItem in newsItems)
                            {
                                // Construir la ruta completa de la imagen
                                var imagePath = Path.Combine(newsItem.ImagePath, newsItem.ImageName);

                                // Verificar si la imagen existe
                                if (!File.Exists(imagePath))
                                {
                                    // Si la imagen no existe, puedes decidir incluir el newsItem sin la imagen
                                    // o indicar de alguna forma que la imagen no está disponible.
                                    newsItemsWithImages.Add(new { NewsItem = newsItem, ImageBase64 = (string)null });
                                    continue;
                                }

                                // Leer la imagen del disco y convertirla en Base64
                                string imageBase64;
                                using (var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                                {
                                    var imageBytes = new byte[imageStream.Length];
                                    await imageStream.ReadAsync(imageBytes, 0, imageBytes.Length);
                                    imageBase64 = Convert.ToBase64String(imageBytes);
                                }

                                // Añadir el newsItem y la imagen codificada en Base64 a la lista
                                newsItemsWithImages.Add(new { NewsItem = newsItem, ImageBase64 = imageBase64 });
                            }

                            

                        });

                        return newsItemsWithImages;

                        //if (newsItems.Count == 0)
                        //return null;
                        // Agregar la imagen en base64 a la lista
                        //     base64Images.Add(base64Image);

                        // return base64Images;
                    }
                    else
                    {
                        return null; // Si la suplantación falla
                    }
                }
                finally
                {
                    // Asegúrate de restablecer la identidad
                    identity?.Dispose();
                }
            }


        }

        public static async Task<List<object?>> GetNewsListBase64OptimizedAsync(List<NewsDto> newsItems, IConfiguration configuration)
        {
            var newsItemsWithImages = new ConcurrentBag<object?>();
            var filesConnection = configuration.GetSection("FilesConnection");

            // Utiliza Parallel.ForEachAsync para un procesamiento paralelo más eficiente
            await Parallel.ForEachAsync(newsItems, async (newsItem, _) =>
            {
                var imagePath = Path.Combine(newsItem.ImagePath, newsItem.ImageName);
                byte[] imageBytes = null;

                // Crear una instancia del ayudante de suplantación para cada archivo
                using (var impersonationHelper = new ImpersonationHelper())
                {
                    WindowsIdentity? identity = null;
                    bool isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        // Verificar si la imagen existe y leerla bajo la suplantación de identidad
                        WindowsIdentity.RunImpersonated(identity.AccessToken, () =>
                        {
                            if (File.Exists(imagePath))
                            {
                                using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                                {
                                    imageBytes = new byte[fileStream.Length];
                                    fileStream.Read(imageBytes, 0, imageBytes.Length);
                                }
                            }
                        });

                        // Liberar la identidad tan pronto como la operación de archivo se haya completado
                        identity.Dispose();
                    }
                }

                // Procesar la imagen fuera del bloque de suplantación
                if (imageBytes != null)
                {
                    try
                    {
                        string imageBase64;
                        using (var ms = new MemoryStream())
                        {
                            using (var originalImage = DrawingImage.FromStream(new MemoryStream(imageBytes)))
                            {
                                var jpegCodec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
                                var encoderParams = new EncoderParameters(1)
                                {
                                    Param = { [0] = new EncoderParameter(Encoder.Quality, 50L) }
                                };

                                originalImage.Save(ms, jpegCodec, encoderParams);
                            }
                            imageBase64 = Convert.ToBase64String(ms.ToArray());
                            newsItemsWithImages.Add(new { NewsItem = newsItem, ImageBase64 = imageBase64 });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al procesar la imagen: {ex.Message}");
                        newsItemsWithImages.Add(new { NewsItem = newsItem, ImageBase64 = (string)null });
                    }
                }
                else
                {
                    newsItemsWithImages.Add(new { NewsItem = newsItem, ImageBase64 = (string)null });
                }
            });

            return newsItemsWithImages.ToList();
        }




        //public static async Task<List<object?>> GetNewsListBase64OptimizedAsync(List<NewsDto> newsItems, IConfiguration configuration)
        //{
        //    using (var impersonationHelper = new ImpersonationHelper())
        //    {
        //        WindowsIdentity? identity = null;
        //        bool isImpersonated = false;

        //        try
        //        {
        //            var filesConnection = configuration.GetSection("FilesConnection");
        //            // Impersonar al usuario una sola vez antes de procesar los archivos
        //            isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

        //            if (isImpersonated && identity != null)
        //            {
        //                // Ejecutar la suplantación una vez para todas las operaciones
        //                return await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
        //                {
        //                    var newsItemsWithImages = new ConcurrentBag<object?>();

        //                    // Utiliza Parallel.ForEachAsync para un procesamiento paralelo más eficiente
        //                    await Parallel.ForEachAsync(newsItems, async (newsItem, _) =>
        //                    {
        //                        var imagePath = Path.Combine(newsItem.ImagePath, newsItem.ImageName);

        //                        // Verificar si la imagen existe
        //                        if (!System.IO.File.Exists(imagePath))
        //                        {
        //                            newsItemsWithImages.Add(new { NewsItem = newsItem, ImageBase64 = (string)null });
        //                            return;
        //                        }

        //                        try
        //                        {
        //                            // Leer y comprimir la imagen a JPEG con calidad reducida
        //                            using (var ms = new MemoryStream())
        //                            {
        //                                using (var originalImage = DrawingImage.FromFile(imagePath))
        //                                {
        //                                    var qualityParam = new EncoderParameter(Encoder.Quality, 50L);
        //                                    var jpegCodec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
        //                                    var encoderParams = new EncoderParameters(1);
        //                                    encoderParams.Param[0] = qualityParam;

        //                                    originalImage.Save(ms, jpegCodec, encoderParams);
        //                                }

        //                                var imageBytes = ms.ToArray();
        //                                string imageBase64 = Convert.ToBase64String(imageBytes);
        //                                newsItemsWithImages.Add(new { NewsItem = newsItem, ImageBase64 = imageBase64 });
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            // Manejo de errores específicos si la imagen falla al procesarse
        //                            Console.WriteLine($"Error al procesar la imagen: {ex.Message}");
        //                            newsItemsWithImages.Add(new { NewsItem = newsItem, ImageBase64 = (string)null });
        //                        }
        //                    });

        //                    return newsItemsWithImages.ToList<object?>();
        //                });
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }
        //        finally
        //        {
        //            identity?.Dispose();
        //        }
        //    }
        //}

        //public static async Task<List<NewsDto>> GetNewsListBase64Async2(List<NewsDto> newsItems, IConfiguration configuration)
        //{
        //    // var base64Images = new List<string>();

        //    using (var impersonationHelper = new ImpersonationHelper())
        //    {
        //        WindowsIdentity? identity = null;
        //        bool isImpersonated = false;

        //        try
        //        {

        //            //         string? base64Image = null;

        //            var filesConnection = configuration.GetSection("FilesConnection");
        //            // Impersonar al usuario antes de intentar acceder al archivo
        //            isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

        //            if (isImpersonated && identity != null)
        //            {
        //                //                var newsItemsWithImages = new List<NewsDto>();

        //                await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
        //                {

        //                    foreach (var newsItem in newsItems)
        //                    {
        //                        // Construir la ruta completa de la imagen
        //                        var imagePath = Path.Combine(newsItem.ImagePath, newsItem.ImageName);

        //                        // Verificar si la imagen existe
        //                        // Verificar si la imagen existe
        //                        if (File.Exists(imagePath))
        //                        {
        //                            // Leer la imagen del disco y convertirla en Base64
        //                            using (var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
        //                            {
        //                                var imageBytes = new byte[imageStream.Length];
        //                                await imageStream.ReadAsync(imageBytes, 0, imageBytes.Length);
        //                                newsItem.ImageBase64 = Convert.ToBase64String(imageBytes);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            // Si la imagen no existe, puedes decidir incluir el newsItem sin la imagen
        //                            // o indicar de alguna forma que la imagen no está disponible.
        //                            newsItem.ImageBase64 = null;
        //                        }
        //                    }



        //                });

        //                return newsItems;

        //                //if (newsItems.Count == 0)
        //                //return null;
        //                // Agregar la imagen en base64 a la lista
        //                //     base64Images.Add(base64Image);

        //                // return base64Images;
        //            }
        //            else
        //            {
        //                return null; // Si la suplantación falla
        //            }
        //        }
        //        finally
        //        {
        //            // Asegúrate de restablecer la identidad
        //            identity?.Dispose();
        //        }
        //    }


        //}



        public static async Task<List<object?>> GetNewsListBase64AppAsync(List<NewsDto> newsItems, IConfiguration configuration)
        {
            var newsItemsWithImages = new List<object>();
            var filesConnection = configuration.GetSection("FilesConnection");
            WindowsIdentity? identity = null;
            bool isImpersonated = false;

            try
            {
                foreach (var newsItem in newsItems)
                {
                    var imagePath = Path.Combine(newsItem.ImagePath, newsItem.ImageName);

                    isImpersonated = new ImpersonationHelper().ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    // Usar impersonation solo al verificar si existe el archivo
                    if (isImpersonated && identity != null)
                    {
                        // Verificar la existencia del archivo
                        bool fileExists = false;

                        await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {
                            fileExists = File.Exists(imagePath);
                        });

                        // Si el archivo no existe, añadir la noticia sin imagen
                        if (!fileExists)
                        {
                            newsItemsWithImages.Add(new { NewsItem = newsItem, ImageBase64 = (string)null });
                            continue;
                        }
                    }
                    else
                    {
                        // Si la suplantación falla, continuar con la ejecución
                        newsItemsWithImages.Add(new { NewsItem = newsItem, ImageBase64 = (string)null });
                        continue;
                    }

                    // Leer el archivo y redimensionar la imagen
                    using (var image = await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                    {
                        return DrawingImage.Load(imagePath);
                    }))
                    {
                        identity?.Dispose();
                        // Redimensionar la imagen proporcionalmente
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(300, 0) // Se establece el ancho a 300 y el alto se calcula automáticamente
                        }));

                        // Convertir la imagen redimensionada en Base64
                        using (var ms = new MemoryStream())
                        {
                            image.SaveAsJpeg(ms); // Guardar imagen como JPEG
                            var imageBytes = ms.ToArray();
                            var imageBase64 = Convert.ToBase64String(imageBytes);
                            newsItemsWithImages.Add(new { NewsItem = newsItem, ImageBase64 = imageBase64 });
                        }
                    }
                }

                return newsItemsWithImages;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return null; // Manejo adicional de errores
            }
            finally
            {
           //     identity?.Dispose(); // Asegúrate de liberar la identidad
            }
        }




        public static async Task<List<NewsDto?>> GetNewsDtoListBase64Async(List<NewsDto> newsItems, IConfiguration configuration)
        {
            // var base64Images = new List<string>();

            using (var impersonationHelper = new ImpersonationHelper())
            {
                WindowsIdentity? identity = null;
                bool isImpersonated = false;

                try
                {

                    string? base64Image = null;

                    var filesConnection = configuration.GetSection("FilesConnection");
                    // Impersonar al usuario antes de intentar acceder al archivo
                    isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        var newsItemsWithImages = new List<NewsDto>();

                        await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {

                            foreach (var newsItem in newsItems)
                            {
                                var newsDto = new NewsDto
                                {
                                    NewId = newsItem.NewId,
                                    NewTitle = newsItem.NewTitle ?? string.Empty,
                                    NewDescription = newsItem.NewDescription ?? string.Empty,
                                    NewTypeId = newsItem.NewTypeId,
                                    ImagePath = newsItem.ImagePath ?? string.Empty,
                                    ImageName = newsItem.ImageName ?? string.Empty,
                                    InitialDate = newsItem.InitialDate,
                                    FinalDate = newsItem.FinalDate,
                                    Priority = newsItem.Priority,
                                    Deleted = newsItem.Deleted
                                };

                                // Construir la ruta completa de la imagen
                                var imagePath = Path.Combine(newsItem.ImagePath ?? string.Empty, newsItem.ImageName ?? string.Empty);

                                if (File.Exists(imagePath))
                                {
                                    // Leer la imagen del disco y convertirla en Base64
                                    using (var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                                    {
                                        var imageBytes = new byte[imageStream.Length];
                                        await imageStream.ReadAsync(imageBytes, 0, imageBytes.Length);
                                        newsDto.ImageBase64 = Convert.ToBase64String(imageBytes);
                                    }
                                }
                                else
                                {
                                    // Si la imagen no existe, establecer ImageBase64 como null
                                    newsDto.ImageBase64 = null;
                                }

                                newsItemsWithImages.Add(newsDto);
                            }

                        });

                        return newsItemsWithImages;

                        //if (newsItems.Count == 0)
                        //return null;
                        // Agregar la imagen en base64 a la lista
                        //     base64Images.Add(base64Image);

                        // return base64Images;
                    }
                    else
                    {
                        return null; // Si la suplantación falla
                    }
                }
                finally
                {
                    // Asegúrate de restablecer la identidad
                    identity?.Dispose();
                }
            }


        }


        // Lista de extensiones de imagen
        private static readonly HashSet<string> imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png"
        };

        // Lista de extensiones PDF
        private static readonly string pdfExtension = ".pdf";

        // Método que verifica si una extensión corresponde a una imagen o PDF
        // Retorna 1 si es imagen, 2 si es PDF, y 0 si no es válido
        public static int GetNewsTypeId(string file)
        {

            /*
            * Si la extensión es nula, vacía o solo contiene espacios en blanco,
            * se considera que no es válida y se devuelve 0
            * Si la extensión es de tipo imagen, se devuelve 1
            * Si la extensión es de tipo PDF, se devuelve 2
            */
            var fileExtension = Path.GetExtension(file);
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                return 0;
            }

            // Asegurarse de que la extensión comience con un punto
            if (!fileExtension.StartsWith("."))
            {
                fileExtension = "." + fileExtension;
            }

            // Si es una imagen, devuelve 1
            if (imageExtensions.Contains(fileExtension))
            {
                return 1;
            }

            // Si es un PDF, devuelve 2
            if (fileExtension.Equals(pdfExtension, StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            // Si no es imagen ni PDF, devuelve 0
            return 0;
        }

        public static async Task<bool> DeleteFileNewsAsync(IConfiguration configuration, string fileName)
        {
            using (var impersonationHelper = new ImpersonationHelper())
            {
                WindowsIdentity? identity = null;
                bool isImpersonated = false;

                try
                {
                    var filesConnection = configuration.GetSection("FilesConnection");
                    // Impersonar al usuario antes de intentar acceder al archivo
                    isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {

                            var ticketsPath = configuration["FilePaths:AppConectados_News"];
                            var filePath = Path.Combine(ticketsPath, fileName);

                            if (File.Exists(filePath))
                            {
                                // Elimina el archivo si existe
                                await Task.Run(() => File.Delete(filePath));
                            }
                             // Indica que el archivo fue eliminado correctamente
                        });

                        return true;
                        //}
                        //else
                        //{
                        //    return false; // Indica que el archivo no existe
                        //}
                    }
                    else
                    {
                        return false; // Si la suplantación falla
                    }
                }
                finally
                {
                    // Asegúrate de restablecer la identidad
                    identity?.Dispose();
                }
            }
        }

        public static async Task<NEW_News?> CreateFileNewsAsync(IConfiguration configuration, NewsDto newsItemDto)
        {
            using (var impersonationHelper = new ImpersonationHelper())
            {
                WindowsIdentity? identity = null;
                bool isImpersonated = false;
                NEW_News newsItem = newsItemDto.ToEntity();
                var imageBase64 = newsItemDto.ImageBase64;

                try
                {
                    var filesConnection = configuration.GetSection("FilesConnection");
                    // Impersonar al usuario antes de intentar acceder al archivo
                    isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {

                            var newsPath = configuration["FilePaths:AppConectados_News"];
                            newsItem.ImagePath = newsPath;

                            if (!string.IsNullOrEmpty(imageBase64))
                            {
                                // Generar un nombre de archivo único usando Guid
                                var originalExtension = Path.GetExtension(newsItem.ImageName);
                                var uniqueFileName = $"{Guid.NewGuid()}{originalExtension}";
                                newsItem.ImageName = uniqueFileName;

                                // Convertir la cadena Base64 a bytes y definir la ruta de guardado del archivo
                                var imageBytes = Convert.FromBase64String(imageBase64);
                                var filePath = Path.Combine(newsPath, uniqueFileName);
                                var directory = Path.GetDirectoryName(filePath);

                                if (!Directory.Exists(directory))
                                {
                                    Directory.CreateDirectory(directory);
                                }

                                // Guardar el archivo en el disco
                                await File.WriteAllBytesAsync(filePath, imageBytes);

                                newsItem.ImagePath = newsPath;
                                newsItem.ImageName = uniqueFileName;

                            }

                            // Procesar imágenes por lenguaje
                            foreach (var lang in newsItemDto.NewsLanguages)
                            {
                                //var languageFileName = $"{Guid.NewGuid()}{Path.GetExtension(lang.ImageName)}";
                                //lang.ImagePath = Path.Combine(newsPath, languageFileName);

                                //var languageImageBytes = Convert.FromBase64String(lang.ImageBase64 ?? string.Empty);
                                //await File.WriteAllBytesAsync(lang.ImagePath, languageImageBytes);

                                //lang.ImageName = languageFileName;

                                if (!string.IsNullOrEmpty(lang.ImageBase64))  // Verificar si hay una imagen en base64
                                {
                                    var languageFileName = $"{Guid.NewGuid()}{Path.GetExtension(lang.ImageName)}";
                                    var languageFilePath = Path.Combine(newsPath, languageFileName);

                                    var languageImageBytes = Convert.FromBase64String(lang.ImageBase64);
                                    await File.WriteAllBytesAsync(languageFilePath, languageImageBytes);

                                    // Asignar la ruta e imagen procesada a la entidad del lenguaje
                                    lang.ImagePath = newsPath;
                                    lang.ImageName = languageFileName;
                                }
                            }

                            
                            
                        });

                        return newsItem;
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    // Asegúrate de restablecer la identidad
                    identity?.Dispose();
                }
            }
        }

        public static async Task<NEW_News?> CreateFileNewsForUpdateAsync(IConfiguration configuration, NewsDto newsItemDto)
        {
            using (var impersonationHelper = new ImpersonationHelper())
            {
                WindowsIdentity? identity = null;
                bool isImpersonated = false;
                NEW_News newsItem = newsItemDto.ToEntity();

                try
                {
                    var filesConnection = configuration.GetSection("FilesConnection");
                    // Impersonar al usuario antes de intentar acceder al archivo
                    isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {
                            // Reutilizar la función para guardar archivos
                            await SaveNewsAndLanguageFilesAsync(configuration, newsItemDto, newsItem);
                        });

                        return newsItem;
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    identity?.Dispose();
                }
            }
        }

        public static async Task SaveNewsAndLanguageFilesAsync(IConfiguration configuration, NewsDto newsItemDto, NEW_News newsItem)
        {
            var newsPath = configuration["FilePaths:AppConectados_News"];

            // Procesar la imagen principal si se proporciona
            if (!string.IsNullOrWhiteSpace(newsItemDto.ImageBase64))
            {
                var originalExtension = Path.GetExtension(newsItemDto.ImageName);
                var uniqueFileName = $"{Guid.NewGuid()}{originalExtension}";
                var filePath = Path.Combine(newsPath, uniqueFileName);

                // Convertir la cadena Base64 a bytes y guardar el archivo
                var imageBytes = Convert.FromBase64String(newsItemDto.ImageBase64);
                await File.WriteAllBytesAsync(filePath, imageBytes);

                // Actualizar el modelo de noticia con los nuevos valores de imagen
                newsItem.ImagePath = filePath;
                newsItem.ImageName = uniqueFileName;
            }

            // Procesar imágenes por lenguaje si se proporcionan
            foreach (var lang in newsItemDto.NewsLanguages)
            {
                if (!string.IsNullOrEmpty(lang.ImageBase64)) // Verificar si hay una imagen en base64
                {
                    var languageFileName = $"{Guid.NewGuid()}{Path.GetExtension(lang.ImageName)}";
                    var languageFilePath = Path.Combine(newsPath, languageFileName);

                    var languageImageBytes = Convert.FromBase64String(lang.ImageBase64);
                    await File.WriteAllBytesAsync(languageFilePath, languageImageBytes);

                    // Asignar la ruta e imagen procesada a la entidad del lenguaje
                    lang.ImagePath = newsPath;
                    lang.ImageName = languageFileName;
                }
            }
        }


        public static async Task<bool> UpdateLanguageImageAsync(IConfiguration configuration, NewsLanguageDto langDto, NewsLanguageDto originLang)
        {
            using (var impersonationHelper = new ImpersonationHelper())
            {
                WindowsIdentity? identity = null;
                bool isImpersonated = false;
                try
                {
                    var filesConnection = configuration.GetSection("FilesConnection");
                    isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        return await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {
                            var newsPath = configuration["FilePaths:AppConectados_News"];

                            var originBase64 = originLang?.ImageBase64 ?? "";
                            // Si la imagen base64 es diferente y no está vacía, procesar y actualizar
                            if (!string.IsNullOrWhiteSpace(langDto.ImageBase64) && langDto.ImageBase64 != originBase64)
                            {
                                // Generar un nombre de archivo único
                                var originalExtension = Path.GetExtension(langDto.ImageName);
                                var uniqueFileName = $"{Guid.NewGuid()}{originalExtension}";
                                var filePath = Path.Combine(newsPath, uniqueFileName);

                                //buscar imagen actual en disco
                                //  var imagePathOru = Path.Combine(originLang.ImagePath ?? string.Empty, originLang.ImageName ?? string.Empty);
                                var imagePathOru = originLang != null ? Path.Combine(originLang.ImagePath ?? string.Empty, originLang.ImageName ?? string.Empty) : string.Empty;
                                var ImageBase64Ori = "";
                                if (File.Exists(imagePathOru))
                                {
                                    // Leer la imagen del disco y convertirla en Base64
                                    using (var imageStreamOri = new FileStream(imagePathOru, FileMode.Open, FileAccess.Read))
                                    {
                                        var imageBytesOri = new byte[imageStreamOri.Length];
                                        await imageStreamOri.ReadAsync(imageBytesOri, 0, imageBytesOri.Length);
                                        ImageBase64Ori = Convert.ToBase64String(imageBytesOri);
                                    }
                                }

                                if (ImageBase64Ori != langDto.ImageBase64)
                                {
                                    // Convertir base64 a bytes y guardar el archivo
                                    var imageBytes = Convert.FromBase64String(langDto.ImageBase64);
                                    await File.WriteAllBytesAsync(filePath, imageBytes);

                                    // Actualizar las propiedades del DTO con los nuevos valores
                                    langDto.ImagePath = newsPath;
                                    langDto.ImageName = uniqueFileName;

                                    // Si el archivo antiguo es diferente, considerar eliminarlo
                                    //if (!string.IsNullOrWhiteSpace(originLang.ImagePath) && originLang.ImagePath != filePath)
                                    //{
                                    //    //File.Delete(originLang.ImagePath);
                                    //}
                                }

                                return true;
                            }

                            return false;
                        });
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
                finally
                {
                    identity?.Dispose();
                }
            }
        }

        public static async Task<bool> SaveImageFromBase64(IConfiguration configuration, string base64Image, string imagePath, string imageName)
        {

            using (var impersonationHelper = new ImpersonationHelper())
            {
                WindowsIdentity? identity = null;
                bool isImpersonated = false;

                try
                {
                    var filesConnection = configuration.GetSection("FilesConnection");
                    // Impersonar al usuario antes de intentar acceder al archivo
                    isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {

                            imagePath = configuration["FilePaths:Images"]; // Asegúrate de definir esta ruta en appsettings
                            if (!Directory.Exists(imagePath))
                                Directory.CreateDirectory(imagePath);

                            var imageBytes = Convert.FromBase64String(base64Image);
                            imageName = $"{Guid.NewGuid()}_{imageName}";
                            var filePath = Path.Combine(imagePath, imageName);

                            await File.WriteAllBytesAsync(filePath, imageBytes);

                            // Devuelve la ruta completa del archivo guardado
                        });

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                finally
                {
                    identity?.Dispose();
                }
            }
        }

        public static async Task<bool> SaveOuteletImages(IConfiguration configuration, OutletDataDto outletData, OUT_Outlet outlet)
        {

            using (var impersonationHelper = new ImpersonationHelper())
            {
                WindowsIdentity? identity = null;
                bool isImpersonated = false;

                try
                {
                    var filesConnection = configuration.GetSection("FilesConnection");
                    // Impersonar al usuario antes de intentar acceder al archivo
                    isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {

                            var imagePath = configuration["FilePaths:AppConectados_Outlets"]; // Asegúrate de definir esta ruta en appsettings
                            if (!Directory.Exists(imagePath))
                                Directory.CreateDirectory(imagePath);
                            outlet.ImagePath = imagePath;

                            if (outletData.ImageBase64_1 != null)
                            {
                                var originalExtension = Path.GetExtension(outletData.ImageName1) ?? ".jpg";
                                outlet.ImageName1 = $"{Guid.NewGuid()}{originalExtension}";
                                var imageBytes = Convert.FromBase64String(outletData.ImageBase64_1);
  //                              outlet.ImageName1 = $"{Guid.NewGuid()}_{outletData.ImageName1}";
                                var filePath = Path.Combine(imagePath, outlet.ImageName1);

                                await File.WriteAllBytesAsync(filePath, imageBytes);
                            }

                            if (outletData.ImageBase64_2 != null)
                            {
                                var originalExtension = Path.GetExtension(outletData.ImageName2) ?? ".jpg";
                                outlet.ImageName2 = $"{Guid.NewGuid()}{originalExtension}";
                                var imageBytes = Convert.FromBase64String(outletData.ImageBase64_2);
 //                               outlet.ImageName2 = $"{Guid.NewGuid()}_{outletData.ImageName2}";
                                var filePath = Path.Combine(imagePath, outlet.ImageName2);

                                await File.WriteAllBytesAsync(filePath, imageBytes);
                            }

                            if (outletData.ImageBase64_3 != null)
                            {
                                var originalExtension = Path.GetExtension(outletData.ImageName3) ?? ".jpg";
                                outlet.ImageName3 = $"{Guid.NewGuid()}{originalExtension}";
                                var imageBytes = Convert.FromBase64String(outletData.ImageBase64_3);
 //                               outlet.ImageName3 = $"{Guid.NewGuid()}_{outletData.ImageName3}";
                                var filePath = Path.Combine(imagePath, outlet.ImageName3);

                                await File.WriteAllBytesAsync(filePath, imageBytes);
                            }


                        });

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                { return false; }   
                finally
                {
                    identity?.Dispose();
                }
            }
        }

        public static async Task<ImageUpdateResult> UpdateOutletImages(IConfiguration configuration, OutletDataDto outletData, OUT_Outlet outlet)
        {
            using (var impersonationHelper = new ImpersonationHelper())
            {
                WindowsIdentity? identity = null;
                bool isImpersonated = false;

                try
                {
                    var filesConnection = configuration.GetSection("FilesConnection");
                    isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

                    if (isImpersonated && identity != null)
                    {
                        bool changesMade = false;

                        await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                        {
                            var imagePath = configuration["FilePaths:AppConectados_Outlets"];
                            if (!Directory.Exists(imagePath))
                                Directory.CreateDirectory(imagePath);

                            async Task<bool> handleImageUpdate(string base64, string imageName, Action<string> setImageName)
                            {
                                string oldFilePath = !string.IsNullOrEmpty(imageName)
                                    ? Path.Combine(imagePath, imageName)
                                    : null;

                                if (string.IsNullOrEmpty(base64))
                                {
                                    // Si base64 está vacío o es null, verifica si existe un archivo anterior y elimínalo.
                                    if (!string.IsNullOrEmpty(imageName) && File.Exists(oldFilePath))
                                    {
                                        File.Delete(oldFilePath); // Elimina el archivo físico
                                        setImageName(null); // Resetea el nombre del archivo a null en el objeto
                                    }
                                    return true; // Señala que hubo un cambio (eliminación de imagen)
                                }
                                else
                                //if (!string.IsNullOrEmpty(base64))
                                {
                                    var originalExtension = !string.IsNullOrEmpty(imageName)
                                                             ? Path.GetExtension(imageName)
                                                             : ".jpg";
                                    var newFileName = $"{Guid.NewGuid()}{originalExtension}";
                                    var newFilePath = Path.Combine(imagePath, newFileName);
                                    var imageBytes = Convert.FromBase64String(base64);

                                    // Compara imágenes si existe una previa
                                    //var oldFilePath = Path.Combine(imagePath, imageName);
                                    if (!string.IsNullOrEmpty(oldFilePath) && File.Exists(oldFilePath))
                                    {
                                        var oldImageBytes = await File.ReadAllBytesAsync(oldFilePath);
                                        var oldBase64 = Convert.ToBase64String(oldImageBytes);
                                        if (oldBase64 == base64) // Si son iguales, no hacer nada
                                            return false; // No changes
                                    }

                                    await File.WriteAllBytesAsync(newFilePath, imageBytes);
                                    setImageName(newFileName); // Actualizar el nombre de la imagen en el objeto outlet

                                    if (!string.IsNullOrEmpty(oldFilePath) && File.Exists(oldFilePath)) // Eliminar el archivo antiguo si se crea uno nuevo
                                        File.Delete(oldFilePath);

                                    return true; // Changes were made
                                }
                                return false; // No changes
                            }

                            changesMade |= await handleImageUpdate(outletData.ImageBase64_1, outlet.ImageName1, newName => outlet.ImageName1 = newName);
                            changesMade |= await handleImageUpdate(outletData.ImageBase64_2, outlet.ImageName2, newName => outlet.ImageName2 = newName);
                            changesMade |= await handleImageUpdate(outletData.ImageBase64_3, outlet.ImageName3, newName => outlet.ImageName3 = newName);
                        });

                        return changesMade ? ImageUpdateResult.Success : ImageUpdateResult.NoChanges;
                    }
                    else
                    {
                        return ImageUpdateResult.Error;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating images: {ex.Message}");
                    return ImageUpdateResult.Error;
                }
                finally
                {
                    identity?.Dispose();
                }
            }
        }


        //public static async Task<bool> UpdateOutletImages(IConfiguration configuration, OutletDataDto outletData, OUT_Outlet outlet)
        //{
        //    using (var impersonationHelper = new ImpersonationHelper())
        //    {
        //        WindowsIdentity? identity = null;
        //        bool isImpersonated = false;

        //        try
        //        {
        //            var filesConnection = configuration.GetSection("FilesConnection");
        //            // Impersonar al usuario antes de intentar acceder al archivo
        //            isImpersonated = impersonationHelper.ImpersonateUser(filesConnection["User"], filesConnection["Domain"], filesConnection["Password"], out identity);

        //            if (isImpersonated && identity != null)
        //            {
        //                await WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
        //                {
        //                    var imagePath = configuration["FilePaths:Images"]; // Asegúrate de definir esta ruta en appsettings
        //                    if (!Directory.Exists(imagePath))
        //                        Directory.CreateDirectory(imagePath);

        //                    // Función auxiliar para procesar y actualizar las imágenes
        //                    async Task handleImageUpdate(string base64, string imageName, Action<string> setImageName)
        //                    {
        //                        if (!string.IsNullOrEmpty(base64))
        //                        {
        //                            var originalExtension = Path.GetExtension(imageName) ?? ".jpg";
        //                            var newFileName = $"{Guid.NewGuid()}{originalExtension}";
        //                            var newFilePath = Path.Combine(imagePath, newFileName);
        //                            var imageBytes = Convert.FromBase64String(base64);

        //                            // Comparar imágenes si existe una previa
        //                            var oldFilePath = Path.Combine(imagePath, imageName);
        //                            if (File.Exists(oldFilePath))
        //                            {
        //                                var oldImageBytes = await File.ReadAllBytesAsync(oldFilePath);
        //                                var oldBase64 = Convert.ToBase64String(oldImageBytes);
        //                                if (oldBase64 == base64) // Si son iguales, no hacer nada
        //                                    return;
        //                            }

        //                            await File.WriteAllBytesAsync(newFilePath, imageBytes);
        //                            setImageName(newFileName); // Actualizar el nombre de la imagen en el objeto outlet

        //                            if (File.Exists(oldFilePath)) // Eliminar el archivo antiguo si se crea uno nuevo
        //                                File.Delete(oldFilePath);
        //                        }
        //                    }

        //                    await handleImageUpdate(outletData.ImageBase64_1, outlet.ImageName1, newName => outlet.ImageName1 = newName);
        //                    await handleImageUpdate(outletData.ImageBase64_2, outlet.ImageName2, newName => outlet.ImageName2 = newName);
        //                    await handleImageUpdate(outletData.ImageBase64_3, outlet.ImageName3, newName => outlet.ImageName3 = newName);
        //                });

        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error updating images: {ex.Message}");
        //            return false;
        //        }
        //        finally
        //        {
        //            identity?.Dispose();
        //        }
        //    }
        //}






    }
}