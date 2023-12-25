using API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddControllers();

var app = builder.Build();

// app.UseCors(builder => builder.AllowAnyHeader().WithOrigins("https://hoppscotch.io"));
app.UseCors(corsBuilder => corsBuilder.AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();