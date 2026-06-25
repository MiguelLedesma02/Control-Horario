using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ControlHorario.Application.Interfaces;
using ControlHorario.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace ControlHorario.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

public class JwtService : IJwtService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _horas;

    public JwtService(string key, string issuer, string audience, int horas)
    {
        _key = key; _issuer = issuer; _audience = audience; _horas = horas;
    }

    public string GenerarToken(Usuario u)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, u.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, u.Email),
            new(ClaimTypes.Name, u.Nombre),
            new(ClaimTypes.Role, u.Rol.ToString())
        };
        if (u.EmpleadoId.HasValue)
            claims.Add(new Claim("EmpleadoId", u.EmpleadoId.Value.ToString()));

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer, audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_horas),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
