using ControlHorario.Application.DTOs;
using ControlHorario.Application.Interfaces;

namespace ControlHorario.Application.Services;

public class AuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;

    public AuthService(IUsuarioRepository usuarios, IPasswordHasher hasher, IJwtService jwt)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest req)
    {
        var u = await _usuarios.GetByEmailAsync(req.Email);
        if (u is null || !u.Activo) return null;
        if (!_hasher.Verify(req.Password, u.PasswordHash)) return null;

        return new LoginResponse(_jwt.GenerarToken(u), u.Nombre, u.Rol.ToString(), u.EmpleadoId);
    }
}
