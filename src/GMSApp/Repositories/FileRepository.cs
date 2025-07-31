using GMSApp.Data;
using GMSApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GMSApp.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<FileItem> _fileSet;

        public FileRepository(AppDbContext context)
        {
            _context = context;
            _fileSet = context.Set<FileItem>();
        }

        public async Task<IEnumerable<FileItem>> GetAllFilesAsync() => await _fileSet.ToListAsync();

        public async Task<FileItem?> GetFileAsync(int id) => await _fileSet.FindAsync(id);

        public async Task UploadFileAsync(string filePath)
        {
            var fileData = await File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            var fileItem = new FileItem
            {
                FileName = fileName,
                ContentType = "application/octet-stream", // fallback type
                Data = fileData
            };

            await _fileSet.AddAsync(fileItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFileAsync(int id)
        {
            var file = await GetFileAsync(id);
            if (file != null)
            {
                _fileSet.Remove(file);
                await _context.SaveChangesAsync();
            }
        }
    }
}
