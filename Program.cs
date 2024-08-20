using SpotifyApi.Classes;
using SpotifyApi.DependencyInjection;
using SpotifyApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ServiceSettings>(builder.Configuration.GetSection("Services"));

// Add services to the container.

builder.Services.AddHttpClient();
builder.Services
 .ConnectDatabase()
 .AddConfigurations(builder.Configuration)
 .UseCustomLogging()
 .AddServices()
 .AddValidators()
 .AddControllers();

builder.Services.AddTransient<IRequestValidatorService, RequestValidatorService>();

var app = builder.Build();

app.UsePathBase("/api");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.UseRouting();

app.Run();