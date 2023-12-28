using API.Data;
using API.Extensions;
using API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// app.UseCors(builder => builder.AllowAnyHeader().WithOrigins("https://hoppscotch.io"));
// app.UseCors(corsBuilder => corsBuilder.AllowAnyHeader());
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(corsBuilder => corsBuilder.AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedUsers(context);
}
catch(Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}
Console.WriteLine();
Console.WriteLine("https://localhost:5001");
Console.WriteLine();
Console.WriteLine("Server is listening on prot 5001");
Console.WriteLine();
app.Run();