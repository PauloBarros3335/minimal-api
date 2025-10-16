using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enuns;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;


#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<DbContexto>(options =>
    {
        options.UseMySql(
           builder.Configuration.GetConnectionString("mysql"),
           ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
                   );
    });


var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
app.MapPost("/adminitradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
        return Results.Unauthorized();
    else
        return Results.Unauthorized();
}).WithTags("Administradores");


app.MapGet("/adminitradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina);
    foreach (var adm in administradores)
    {
        adms.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok((adms));
}).WithTags("Administradores");


app.MapGet("/adminitradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    if (administrador == null)
        return Results.NotFound();
    return Results.Ok(new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).WithTags("Administradores");


app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var Validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };
    if (string.IsNullOrEmpty(administradorDTO.Email))
        Validacao.Mensagens.Add("O email do administrador é obrigatório");

    if (string.IsNullOrEmpty(administradorDTO.Senha))
        Validacao.Mensagens.Add("A senha do administrador é obrigatória");

    if (administradorDTO.Perfil == null)
        Validacao.Mensagens.Add("O perfil do administrador é obrigatório");

    if (Validacao.Mensagens.Count > 0)
        return Results.BadRequest(Validacao);

    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };

    administradorServico.Incluir(administrador);

    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });

}).WithTags("Administradores");

#endregion

#region Veiculos
ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var Validacao = new ErrosDeValidacao { Mensagens = new List<string>() };
    if (string.IsNullOrEmpty(veiculoDTO.Nome))
        Validacao.Mensagens.Add("O nome do veiculo é obrigatório");

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
        Validacao.Mensagens.Add("A marca do veiculo é obrigatória");

    if (veiculoDTO.Ano < 1950)
        Validacao.Mensagens.Add("Veiculos anteriores a 1950 não são aceitos");

    return Validacao;
}
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
        {

            // var Validacao = validaDTO(veiculoDTO);
            // if (string.IsNullOrEmpty(veiculoDTO.Nome))
            //     Validacao.Mensagens.Add("O nome do veiculo é obrigatório");

            // if (string.IsNullOrEmpty(veiculoDTO.Marca))
            //     Validacao.Mensagens.Add("A marca do veiculo é obrigatória");

            // if (veiculoDTO.Ano < 1950)
            //     Validacao.Mensagens.Add("Veiculos anteriores a 1950 não são aceitos");

            var Validacao = validaDTO(veiculoDTO);
            if (Validacao.Mensagens.Count > 0)
                return Results.BadRequest(Validacao);

            var veiculo = new Veiculo
            {
                Nome = veiculoDTO.Nome,
                Marca = veiculoDTO.Marca,
                Ano = veiculoDTO.Ano
            };
            veiculoServico.Incluir(veiculo);
            return Results.Created($"/veiculo/{veiculo.Id}", veiculo);

        }).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);


}).WithTags("Veiculos");




app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null)
        return Results.NotFound();
    return Results.Ok(veiculo);

}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null)
        return Results.NotFound();

    var Validacao = validaDTO(veiculoDTO);
    if (Validacao.Mensagens.Count > 0)
        return Results.BadRequest(Validacao);


    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);
    return Results.Ok(veiculo);

}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null)
        return Results.NotFound();
    veiculoServico.Apagar(veiculo);
    return Results.NoContent();

}).WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion        

