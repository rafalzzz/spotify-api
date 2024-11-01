using Microsoft.EntityFrameworkCore;
using SpotifyApi.Services;
using SpotifyApi.Utilities;
using SpotifyApi.Variables;

namespace SpotifyApi.Entities
{
    public class SpotifyDbContext(DbContextOptions<SpotifyDbContext> options, IErrorHandlingService errorHandlingService) : DbContext(options)
    {
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));

        public DbSet<User> Users { get; set; }
        public DbSet<Playlist> Playlists { get; set; }

        private static void ConfigureUserEntity(ModelBuilder modelBuilder)
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
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Nickname)
                .IsUnique();
        }

        private static void ConfigurePlaylistEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Playlist>()
                .HasOne(playlist => playlist.Owner)
                .WithMany(user => user.CreatedPlaylists)
                .HasForeignKey(playlist => playlist.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Playlist>()
                .HasMany(playlist => playlist.Collaborators)
                .WithMany(user => user.CollaboratingPlaylists)
                .UsingEntity(joinEntity => joinEntity.ToTable("PlaylistCoCreators"));

            modelBuilder.Entity<Playlist>()
                .HasMany(playlist => playlist.FavoritedByUsers)
                .WithMany(user => user.FavoritePlaylists)
                .UsingEntity(joinEntity => joinEntity.ToTable("UserFavoritePlaylists"));

            modelBuilder.Entity<Playlist>()
                .Property(playlist => playlist.IsPublic)
                .HasConversion<int>();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureUserEntity(modelBuilder);
            ConfigurePlaylistEntity(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.ConnectionString);

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string is not provided.");
                }

                optionsBuilder
                .UseNpgsql(connectionString)
                .UseLazyLoadingProxies();
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