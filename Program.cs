using DotNetEnv;
using SpotifyApi.DependencyInjection;
using SpotifyApi.MappingProfiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(PlaylistProfile));

Env.Load();

builder.Services.AddHttpClient();

builder.Services
    .ConnectDatabase()
    .AddConfigurations(builder.Configuration)
    .UseCustomLogging()
    .AddServices()
    .AddValidators()
    .AddJwtAuthentication(builder.Configuration)
    .AddSwaggerConfiguration()
    .AddControllers();

var app = builder.Build();

app.UseSwaggerConfiguration(app.Environment);

app.UsePathBase("/api");
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();