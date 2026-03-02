using ImageCropper.API.PomocneController;
using Microsoft.Data.SqlClient;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IPomocnaZaProcesiranjeSlike, PomocnaZaProcesiranjeSlike>();

// Registruj connection string
builder.Services.AddScoped<SqlConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodaj CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Čekaj da SQL Server bude spreman
var retries = 10;
while (retries > 0)
{
    try
    {
        using (var connection = new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")
            .Replace("Database=ImageCropperDb;", "Database=master;")))
        {
            connection.Open();
            var cmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ImageCropperDb')
                BEGIN
                    CREATE DATABASE ImageCropperDb;
                END", connection);
            cmd.ExecuteNonQuery();
        }

        using (var connection = new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")))
        {
            connection.Open();
            var cmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Configs' AND xtype='U')
                CREATE TABLE Configs (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    ScaleDown FLOAT NOT NULL,
                    LogoPosition NVARCHAR(50) NOT NULL,
                    LogoImage VARBINARY(MAX) NULL
                )", connection);
            cmd.ExecuteNonQuery();
        }
        break;
    }
    catch
    {
        retries--;
        Console.WriteLine($"Čekam SQL Server... preostalo pokušaja: {retries}");
        Thread.Sleep(5000);
    }
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
