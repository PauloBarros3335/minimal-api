
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos;

public class AdministradorServico : IAdministradorServico
{
    private readonly DbContexto _contexto;
    public AdministradorServico(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public object BuscaPorId(int id)
    {
        throw new NotImplementedException();
    }

    public Administrador? BuscarPorId(int id)
    {
       return _contexto.Administradores.Where(a => a.Id == id).FirstOrDefault();
    }

    public Administrador Incluir(Administrador administrador)
    {
        _contexto.Administradores.Add(administrador);
        _contexto.SaveChanges();

        return administrador;
    }

     public Administrador? Login(LoginDTO loginDTO)
    {
        var adm = _contexto.Administradores.Where(a => a.Email.ToLower() == loginDTO.Email.ToLower() && a.Senha == loginDTO.Senha).FirstOrDefault();
        if (adm == null)
        {
            // Handle the null case as needed, e.g., throw an exception or return null
            return null;
        }
        return adm;
    }

    public List<Administrador> Todos(int? pagina)
    {
        var query = _contexto.Administradores.AsQueryable();
        var tamanhoPagina = 10;

        if (pagina != null)
            query = query.Skip(((int)pagina - 1) * tamanhoPagina).Take(tamanhoPagina);

        return query.ToList();
    }

    // Administrador IAdministradorServico.Incluir(Administrador administrador)
    // {
    //     throw new NotImplementedException();
    // }
}


