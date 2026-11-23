using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace LibrarySystem.Data
{
    public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
    {
        public LibraryDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? "Server=db;Port=3306;Database=library_db;Uid=root;Pwd=123456;";

            Console.WriteLine("DEBUG_CS: " + connectionString);

            var builder = new DbContextOptionsBuilder<LibraryDbContext>();
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new LibraryDbContext(builder.Options);
        }
    }
}