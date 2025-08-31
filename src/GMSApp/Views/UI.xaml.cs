/*using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.job
{
    public class Joborder
    {
        [Key]
        public int Id { get; set; }


        public string? CustomerName { get; set; }


        public string? Phonenumber { get; set; }


        public string? VehicleNumber { get; set; }


        public string? Brand { get; set; }


        public string? Model { get; set; }


        public Decimal? OdoNumber { get; set; }

        public ICollection<ItemRow> Items { get; set; } = new List<ItemRow>();

        public byte[]? F { get; set; }

        public string? FN { get; set; }

        public byte[]? B { get; set; }

        public string? BN { get; set; }

        public byte[]? LS { get; set; }

        public string? LSN { get; set; }
        public byte[]? RS { get; set; }

        public string? RSN { get; set; }

        public DateTime? Created { get; set; } = DateTime.Now;


    }
}
using GMSApp.Models.job;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMSApp.Models;
public class ItemRow
{
    [Key]
    public int Id { get; set; } // for EF
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public decimal Total => Quantity * Price;

    public int PurchaseOrderId { get; set; }

    [ForeignKey(nameof(Joborder.Id))]
    public int Joborderid { get; set; }

    public Joborder Joborder { get; set; }


}


*//*
using GMSApp.Data;
using GMSApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection;
namespace GMSApp.Repositories;

public class GenericPdfGenerator<T> : IGenericPdfGenerator<T> where T : class
{
    public async Task GeneratePdfAsync(IEnumerable<T> items, string filePath)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Content()
                    .Column(column =>
                    {
                        column.Item().Text($"{typeof(T).Name} List").FontSize(18).Bold().Underline();

                        foreach (var item in items)
                        {
                            column.Item().Border(1).Padding(5).Column(row =>
                            {
                                foreach (var prop in properties)
                                {
                                    var value = prop.GetValue(item)?.ToString() ?? "null";
                                    row.Item().Text($"{prop.Name}: {value}");
                                }
                            });
                        }
                    });
            });
        }).GeneratePdf(filePath);
    }
}
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
namespace GMSApp.Repositories;
public interface IGenericPdfGenerator<T> where T : class
{
    Task GeneratePdfAsync(IEnumerable<T> items, string filePath);
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace GMSApp.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);

    }

}
using GMSApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GMSApp.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }

}
<Project Sdk = "Microsoft.NET.Sdk" >

  < PropertyGroup >
    < OutputType > WinExe </ OutputType >
    < TargetFramework > net9.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>

  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include = "CommunityToolkit.Mvvm" Version="8.4.0" />
    <PackageReference Include = "MaterialDesignColors" Version="5.2.1" />
    <PackageReference Include = "MaterialDesignThemes" Version="5.2.1" />
    <PackageReference Include = "Microsoft.EntityFrameworkCore" Version="9.0.7" />
    <PackageReference Include = "Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.7" />
    <PackageReference Include = "Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.7" />
    <PackageReference Include = "Microsoft.EntityFrameworkCore.Tools" Version="9.0.7">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include = "Microsoft.Extensions.DependencyInjection" Version="9.0.7" />
    <PackageReference Include = "Microsoft.Extensions.Hosting" Version="9.0.7" />
    <PackageReference Include = "MimeKit" Version="4.13.0" />
    <PackageReference Include = "QuestPDF" Version="2025.7.0" />
    <PackageReference Include = "Serilog" Version="4.3.0" />
    <PackageReference Include = "Serilog.Extensions.Hosting" Version="9.0.0" />
    <PackageReference Include = "Serilog.Sinks.File" Version="7.0.0" />
  </ItemGroup>

  <ItemGroup>
    <Folder Include = "Assets\" />
    < Folder Include="Resources\" />
    <Folder Include = "Services\" />
  </ ItemGroup >

</ Project >








*/