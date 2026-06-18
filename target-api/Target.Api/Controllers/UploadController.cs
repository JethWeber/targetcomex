using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Target.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("capa")]
    public async Task<IActionResult> UploadCapa(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Ficheiro vazio.");

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("O ficheiro excede 5 MB.");

        var folder = Path.Combine(_env.ContentRootPath, "wwwroot", "Uploads", "Veiculos");
        Directory.CreateDirectory(folder);

        var ext      = Path.GetExtension(file.FileName);
        var nome     = $"{Guid.NewGuid()}{ext}";
        var caminho  = Path.Combine(folder, nome);

        using var stream = System.IO.File.Create(caminho);
        await file.CopyToAsync(stream);

        return Ok($"/Uploads/Veiculos/{nome}");
    }
}