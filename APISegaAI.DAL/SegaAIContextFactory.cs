using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace APISegaAI.DAL
{
    public class SegaAIContextFactory : IDesignTimeDbContextFactory<SegaAIContext>
    {
        public SegaAIContext CreateDbContext(string[] args)
        {
            // Определяем путь к appsettings.json относительно папки APISegaAI.DAL
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../APISegaAI"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<SegaAIContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new SegaAIContext(optionsBuilder.Options);
        }
    }
}