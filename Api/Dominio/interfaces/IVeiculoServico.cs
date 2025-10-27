using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;

namespace MinimalApi.Dominio.Interfaces;

public interface IVeiculoServico
{

    List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null);
    Veiculo? BuscarPorId(int id);
    void Incluir(Veiculo veiculo);
    void Atualizar(Veiculo veiculo);
    void Apagar(Veiculo veiculo);
    object? BuscaPorId(int id);
    void Atualizar(object veiculo);
    void Apagar(object veiculo);
}