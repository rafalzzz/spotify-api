using SpotifyApi.Classes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ServiceSettings>(builder.Configuration.GetSection("Services"));

builder.Services.AddHttpClient();
builder.Services.AddControllers();

var app = builder.Build();

app.UsePathBase("/api");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.UseRouting();

app.Run();