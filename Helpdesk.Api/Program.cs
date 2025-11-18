using Helpdesk.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connStr = builder.Configuration.GetConnectionString("Default")
             ?? "Data Source=helpdesk.db";
builder.Services.AddDbContext<HelpdeskDbContext>(opt =>
    opt.UseSqlite(connStr));

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Ensure DB exists and seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HelpdeskDbContext>();
    await db.Database.EnsureCreatedAsync();
    await Seed.EnsureSeedDataAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();
app.MapRazorPages();

app.Run();

public partial class Program { }
