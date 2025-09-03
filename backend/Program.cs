using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = new[]
{
    "https://elias6969.github.io", 
    "http://localhost:5500"       
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetPreflightMaxAge(TimeSpan.FromHours(1)));
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");

app.MapGet("/", () => Results.Ok("PigeonPass backend is running"));

// DB connection string (fail fast if missing)
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (string.IsNullOrWhiteSpace(connectionString))
{
    app.Logger.LogError("DB_CONNECTION_STRING is not set");
}

// Explicit preflight route so OPTIONS never 404s
app.MapMethods("/api/register", new[] { "OPTIONS" }, () => Results.Ok())
   .RequireCors("FrontendPolicy");

// POST /api/register
app.MapPost("/api/register", async (HttpContext context) =>
{
    try
    {
        var data = await context.Request.ReadFromJsonAsync<Visitor>();
        if (data is null || string.IsNullOrWhiteSpace(data.Name) || string.IsNullOrWhiteSpace(data.Email))
        {
            app.Logger.LogWarning("Invalid input received from client.");
            return Results.BadRequest(new { message = "Invalid input" });
        }

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        const string sql = @"INSERT INTO visitors (name, email, company, purpose)
                             VALUES (@name, @email, @company, @purpose)";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", data.Name);
        cmd.Parameters.AddWithValue("email", data.Email);
        cmd.Parameters.AddWithValue("company", (object?)data.Company ?? DBNull.Value);
        cmd.Parameters.AddWithValue("purpose", (object?)data.Purpose ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();

        // Success logging
        app.Logger.LogInformation("Visitor registered: {Name}, {Email}, {Company}, {Purpose}", 
            data.Name, data.Email, data.Company, data.Purpose);

        return Results.Ok(new { message = $"Welcome {data.Name}, your visit has been registered." });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Register failed");
        return Results.Problem("Could not register visitor");
    }
}).RequireCors("FrontendPolicy");

app.Run();

public record Visitor(string Name, string Email, string? Company, string? Purpose);
