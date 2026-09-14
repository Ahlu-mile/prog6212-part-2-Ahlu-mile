using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------

builder.Services.AddControllers();

// EF Core - Code-First against SQL Server. Connection string lives in
// appsettings.json / appsettings.Development.json.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session support (server-side session state, backed by an in-memory cache
// by default). The session cookie carries only an opaque session ID -
// the actual UserId/Role are stored server-side, per the brief's
// "session management to maintain the user's authenticated state" requirement.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpContextAccessor();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "RESTful API for the RaceDay event management system (Part 2 - PROG6212)."
    });
});

// CORS - open for local MVC front end development in Part 3.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ---------- Middleware pipeline ----------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors();

// Session must be enabled before any middleware/controllers that read it.
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed so RaceDay.Tests can spin up this app via WebApplicationFactory<Program>.
public partial class Program { }
