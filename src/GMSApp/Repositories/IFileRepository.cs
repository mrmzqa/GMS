using GMSApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Repositories
{
    public interface IFileRepository
    {
        Task<IEnumerable<FileItem>> GetAllFilesAsync();
        Task<FileItem?> GetFileAsync(int id);
        Task UploadFileAsync(string filePath);
        Task DeleteFileAsync(int id);
    }

}
