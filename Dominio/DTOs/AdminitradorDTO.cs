using MinimalApi.Dominio.Enuns;

namespace MinimalApi.Dominio.DTOs;

public struct AdministradorDTO
{
    public string Email { get; set; }
    public string Senha { get; set; }
    public Perfil? Perfil { get; set; }
}
