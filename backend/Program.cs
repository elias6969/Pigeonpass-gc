using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
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

app.UseCors();

string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

app.MapPost("/api/register", async (HttpContext context) =>
{
    try
    {
        var data = await context.Request.ReadFromJsonAsync<Visitor>();
        if (data is null)
        {
            return Results.BadRequest(new { message = "Invalid input" });
        }

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        string sql = "INSERT INTO visitors (name, email, company, purpose) VALUES (@name, @email, @company, @purpose)";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", data.Name);
        cmd.Parameters.AddWithValue("email", data.Email);
        cmd.Parameters.AddWithValue("company", (object?)data.Company ?? DBNull.Value);
        cmd.Parameters.AddWithValue("purpose", (object?)data.Purpose ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();

        return Results.Ok(new { message = "Visitor registered!" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {ex.Message}");
        return Results.Problem("Could not register visitor");
    }
});

app.Run();

record Visitor(string Name, string Email, string Company, string Purpose);
