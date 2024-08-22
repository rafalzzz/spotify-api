using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using SpotifyApi.Variables;
using SpotifyApi.Services;
using SpotifyApi.Utilities;

namespace SpotifyApi.Entities
{
    public class SpotifyDbContext(DbContextOptions<SpotifyDbContext> options, IErrorHandlingService errorHandlingService) : DbContext(options)
    {
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(entity => entity.Offers)
                .HasConversion<int>();

            modelBuilder.Entity<User>()
                .Property(entity => entity.ShareInformation)
                .HasConversion<int>();

            modelBuilder.Entity<User>()
                .Property(entity => entity.Terms)
                .HasConversion<int>();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Nickname)
                .IsUnique();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                Env.Load();

                var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.ConnectionString);

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string is not provided.");
                }

                optionsBuilder.UseNpgsql(connectionString);
            }
            catch (Exception exception)
            {
                var logErrorAction = "connect to database";

                _errorHandlingService.HandleError(
                    exception,
                    ErrorType.Internal,
                    logErrorAction
                );

                throw;
            }
        }

        public async Task<bool> UserExists(string email, string nickname)
        {
            try
            {
                return await Users.AnyAsync(x => x.Email == email || x.Nickname == nickname);
            }
            catch (Exception exception)
            {
                _errorHandlingService.HandleDatabaseError(
                    exception,
                    "check if user exists"
                );

                return false;
            }
        }
    }
}