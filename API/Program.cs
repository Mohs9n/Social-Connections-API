using API;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// app.UseCors(builder => builder.AllowAnyHeader().WithOrigins("https://hoppscotch.io"));
// app.UseCors(corsBuilder => corsBuilder.AllowAnyHeader());
app.UseCors(corsBuilder => corsBuilder.AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();