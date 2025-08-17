using System;
using System.IO;
using System.Threading.Tasks;
namespace TindaKo.Services
{


    namespace TindaKo.Services
    {
        public static class FileService
        {
            public static async Task<string> SaveTextToFileAsync(string fileName, string content)
            {
                try
                {
                    // Use the local app data folder (cross-platform safe)
                    var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                    // Write content to file
                    using (var writer = new StreamWriter(filePath, false))
                    {
                        await writer.WriteAsync(content);
                    }

                    return filePath; // return path so you can share or open later
                }
                catch (Exception ex)
                {
                    // log or handle errors if needed
                    Console.WriteLine($"Error saving file: {ex.Message}");
                    throw;
                }
            }
        }
    }

}
