using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ImageCropper.API.Controllers
{
    [ApiController]
    [Route("api/config")]
    public class ConfigController : ControllerBase
    {
        private readonly SqlConnection _connection;

        public ConfigController(SqlConnection connection)
        {
            _connection = connection;
        }
  
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateConfig(
            [FromForm] float scaleDown,
            [FromForm] string logoPosition,
            [FromForm] IFormFile? logoImage)
        {
            if (scaleDown > 0.25f)
                return BadRequest("scaleDown ne može biti veći od 0.25");

            byte[]? logoBytes = null;
            if (logoImage != null)
            {
                using var ms = new MemoryStream();
                await logoImage.CopyToAsync(ms);
                logoBytes = ms.ToArray();
            }

            _connection.Open();
            var cmd = new SqlCommand(
                "INSERT INTO Configs (ScaleDown, LogoPosition, LogoImage) OUTPUT INSERTED.Id VALUES (@s, @l, @img)",
                _connection);
            cmd.Parameters.AddWithValue("@s", scaleDown);
            cmd.Parameters.AddWithValue("@l", logoPosition);
            cmd.Parameters.AddWithValue("@img", logoBytes ?? (object)DBNull.Value);

            var id = (int)await cmd.ExecuteScalarAsync();
            _connection.Close();

            return Ok(new { Id = id, ScaleDown = scaleDown, LogoPosition = logoPosition });
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateConfig(
            int id,
            [FromForm] float scaleDown,
            [FromForm] string logoPosition,
            [FromForm] IFormFile? logoImage)
        {
            if (scaleDown > 0.25f)
                return BadRequest("scaleDown ne može biti veći od 0.25");

            byte[]? logoBytes = null;
            if (logoImage != null)
            {
                using var ms = new MemoryStream();
                await logoImage.CopyToAsync(ms);
                logoBytes = ms.ToArray();
            }

            _connection.Open();

            // Provjeri postoji li config
            var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Configs WHERE Id = @id", _connection);
            checkCmd.Parameters.AddWithValue("@id", id);
            var count = (int)await checkCmd.ExecuteScalarAsync();

            if (count == 0)
            {
                _connection.Close();
                return NotFound("Config nije pronađen");
            }

            var cmd = new SqlCommand(
                "UPDATE Configs SET ScaleDown = @s, LogoPosition = @l, LogoImage = @img WHERE Id = @id",
                _connection);
            cmd.Parameters.AddWithValue("@s", scaleDown);
            cmd.Parameters.AddWithValue("@l", logoPosition);
            cmd.Parameters.AddWithValue("@img", logoBytes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@id", id);

            await cmd.ExecuteNonQueryAsync();
            _connection.Close();

            return Ok(new { Id = id, ScaleDown = scaleDown, LogoPosition = logoPosition });
        }

        [ApiExplorerSettings(IgnoreApi = true)]
public static async Task<(float ScaleDown, string LogoPosition, byte[]? LogoImage)> GetLatestConfig(SqlConnection connection)
        {
            connection.Open();
            var cmd = new SqlCommand(
                "SELECT TOP 1 ScaleDown, LogoPosition, LogoImage FROM Configs ORDER BY Id DESC",
                connection);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var scaleDown = (float)(double)reader["ScaleDown"];
                var logoPosition = reader["LogoPosition"].ToString()!;
                var logoImage = reader["LogoImage"] == DBNull.Value ? null : (byte[])reader["LogoImage"];
                connection.Close();
                return (scaleDown, logoPosition, logoImage);
            }

            connection.Close();
            return (0.25f, "bottom-right", null);
        }
    }
}