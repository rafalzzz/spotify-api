using DotNetEnv;
using SpotifyApi.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddHttpClient();

builder.Services
    .ConnectDatabase()
    .AddConfigurations(builder.Configuration)
    .UseCustomLogging()
    .AddServices()
    .AddValidators()
    .AddJwtAuthentication(builder.Configuration)
    .AddControllers();

var app = builder.Build();

app.UsePathBase("/api");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseRouting();

app.Run();