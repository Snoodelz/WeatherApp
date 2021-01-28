using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace WeatherAppConsole.Models
{
    class EFContext : DbContext
    {
        string connectionString = string.Empty;

        public EFContext() : base()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            connectionString = configuration.GetConnectionString("SomeConnectionString");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        public DbSet<Indoor> Indoors { get; set; }
        public DbSet<Outdoor> Outdoors { get; set; }
    }
}
