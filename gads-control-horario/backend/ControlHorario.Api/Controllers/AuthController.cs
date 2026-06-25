using ControlHorario.Application.DTOs;
using ControlHorario.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControlHorario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    public AuthController(AuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var resp = await _auth.LoginAsync(req);
        if (resp == null) return Unauthorized(new { message = "Credenciales inválidas" });
        return Ok(resp);
    }
}
