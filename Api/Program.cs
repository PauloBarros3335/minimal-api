// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Options;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
// using MinimalApi.Dominio.DTOs;
// using MinimalApi.Dominio.Entidades;
// using MinimalApi.Dominio.Enuns;
// using MinimalApi.Dominio.Interfaces;
// using MinimalApi.Dominio.ModelViews;
// using MinimalApi.Dominio.Servicos;
// using MinimalApi.DTOs;
// using MinimalApi.Infraestrutura.Db;


// #region Builder
// var builder = WebApplication.CreateBuilder(args);

// var Key = builder.Configuration.GetSection("Jwt").ToString();
// if (string.IsNullOrEmpty(Key)) Key = "03048696";

// builder.Services.AddAuthentication(option =>
// {
//     option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// }).AddJwtBearer(option =>
// {
//     option.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateLifetime = true,
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
//         ValidateIssuer = false,
//         ValidateAudience = false,

//     };

// });

// builder.Services.AddAuthorization();

// builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
// builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(Options =>
// {
//     Options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = SecuritySchemeType.Http,
//         Scheme = "Bearer",
//         BearerFormat = "JWT",
//         In = ParameterLocation.Header,
//         Description = "Insira o token JWT aqui ."
//     });

//     Options.AddSecurityRequirement(new OpenApiSecurityRequirement
//     {
//         {
//             new OpenApiSecurityScheme
//             {
//                 Reference = new OpenApiReference
//                 {
//                     Type = ReferenceType.SecurityScheme,
//                     Id = "Bearer"
//                 }
//             },
//             new string[] {}
//         }
//     });

// });



// builder.Services.AddDbContext<DbContexto>(options =>
//     {
//         options.UseMySql(
//            builder.Configuration.GetConnectionString("MySql"),
//            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
//                    );
//     });

// var app = builder.Build();
// #endregion

// #region Home
// app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
// #endregion

// #region Administradores
// string gerarToken(Administrador administrador)
// {
//     if (string.IsNullOrEmpty(Key)) return string.Empty;

//     var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
//     var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

//     var clcims = new List<Claim>()
//     {
//         new Claim("Email", administrador.Email),
//         new Claim("perfil", administrador.Perfil),
//         new Claim(ClaimTypes.Role, administrador.Perfil)
//     };

//     var token = new JwtSecurityToken(
//         claims: clcims,
//         expires: DateTime.Now.AddDays(1),
//         signingCredentials: credentials
//     );
//     return new JwtSecurityTokenHandler().WriteToken(token);
// }

// app.MapPost("/adminitradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
// {
//     var adm = administradorServico.Login(loginDTO);
//     if (adm != null)
//     {

//         string token = gerarToken(adm);
//         return Results.Ok(new AdminitradorLogado
//         {
//             Email = adm.Email,
//             Perfil = adm.Perfil,
//             Token = token
//         });
//     }
//     else
//         return Results.Unauthorized();
// }).AllowAnonymous().WithTags("Administradores");


// app.MapGet("/adminitradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
// {
//     var adms = new List<AdministradorModelView>();
//     var administradores = administradorServico.Todos(pagina);
//     foreach (var adm in administradores)
//     {
//         adms.Add(new AdministradorModelView
//         {
//             Id = adm.Id,
//             Email = adm.Email,
//             Perfil = adm.Perfil
//         });
//     }
// return Results.Ok(adms);  
// })

// .RequireAuthorization()
// .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
// .WithTags("Administradores");


// app.MapGet("/adminitradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
// {
//     var administrador = administradorServico.BuscarPorId(id);
//     if (administrador == null)
//         return Results.NotFound();
//     return Results.Ok(new AdministradorModelView
//     {
//         Id = administrador.Id,
//         Email = administrador.Email,
//         Perfil = administrador.Perfil
//     });
// }).RequireAuthorization()
// .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
// .WithTags("Administradores");


// app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
// {
//     var Validacao = new ErrosDeValidacao
//     {
//         Mensagens = new List<string>()
//     };
//     if (string.IsNullOrEmpty(administradorDTO.Email))
//         Validacao.Mensagens.Add("O email do administrador é obrigatório");

//     if (string.IsNullOrEmpty(administradorDTO.Senha))
//         Validacao.Mensagens.Add("A senha do administrador é obrigatória");

//     if (administradorDTO.Perfil == null)
//         Validacao.Mensagens.Add("O perfil do administrador é obrigatório");

//     if (Validacao.Mensagens.Count > 0)
//         return Results.BadRequest(Validacao);

//     var administrador = new Administrador
//     {
//         Email = administradorDTO.Email,
//         Senha = administradorDTO.Senha,
//         Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
//     };

//     administradorServico.Incluir(administrador);

//     return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView
//     {
//         Id = administrador.Id,
//         Email = administrador.Email,
//         Perfil = administrador.Perfil
//     });

// }).RequireAuthorization()
// .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
// .WithTags("Administradores");

// #endregion

// #region Veiculos
// ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
// {
//     var Validacao = new ErrosDeValidacao { Mensagens = new List<string>() };
//     if (string.IsNullOrEmpty(veiculoDTO.Nome))
//         Validacao.Mensagens.Add("O nome do veiculo é obrigatório");

//     if (string.IsNullOrEmpty(veiculoDTO.Marca))
//         Validacao.Mensagens.Add("A marca do veiculo é obrigatória");

//     if (veiculoDTO.Ano < 1950)
//         Validacao.Mensagens.Add("Veiculos anteriores a 1950 não são aceitos");

//     return Validacao;
// }
// app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
//         {
//             var Validacao = validaDTO(veiculoDTO);
//             if (Validacao.Mensagens.Count > 0)
//                 return Results.BadRequest(Validacao);

//             var veiculo = new Veiculo
//             {
//                 Nome = veiculoDTO.Nome,
//                 Marca = veiculoDTO.Marca,
//                 Ano = veiculoDTO.Ano
//             };
//             veiculoServico.Incluir(veiculo);
//             return Results.Created($"/veiculo/{veiculo.Id}", veiculo);

//         })
//         .RequireAuthorization()
//         .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
//         .WithTags("Veiculos");

// app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
// {
//     var veiculos = veiculoServico.Todos(pagina);
//     return Results.Ok(veiculos);


// }).RequireAuthorization().WithTags("Veiculos");


// app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
// {
//     var veiculo = veiculoServico.BuscarPorId(id);
//     if (veiculo == null)
//         return Results.NotFound();
//     return Results.Ok(veiculo);

// })
// .RequireAuthorization()
// .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
// .WithTags("Veiculos");

// app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
// {

//     var veiculo = veiculoServico.BuscarPorId(id);
//     if (veiculo == null)
//         return Results.NotFound();

//     var Validacao = validaDTO(veiculoDTO);
//     if (Validacao.Mensagens.Count > 0)
//         return Results.BadRequest(Validacao);


//     veiculo.Nome = veiculoDTO.Nome;
//     veiculo.Marca = veiculoDTO.Marca;
//     veiculo.Ano = veiculoDTO.Ano;

//     veiculoServico.Atualizar(veiculo);
//     return Results.Ok(veiculo);

// })
// .RequireAuthorization()
// .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
// .WithTags("Veiculos");

// app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
// {
//     var veiculo = veiculoServico.BuscarPorId(id);
//     if (veiculo == null)
//         return Results.NotFound();
//     veiculoServico.Apagar(veiculo);
//     return Results.NoContent();

// })
// .RequireAuthorization()
// .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
// .WithTags("Veiculos");
// #endregion

// #region App
// app.UseSwagger();
// app.UseSwaggerUI();

// app.UseAuthentication();
// app.UseAuthorization();

// app.Run();
// #endregion        



// using Microsoft.AspNetCore.Hosting;
// using MinimalApi;

// IHostBuilder CreateHostBuilder(string[] args) =>
//     Host.CreateDefaultBuilder(args)
//         .ConfigureWebHostDefaults(webBuilder =>
//         {
//             webBuilder.UseStartup<IStartup>();
//         });
// CreateHostBuilder(args).Build().Run();






// ----------------------------------------------------------------------
// Este código deve substituir o conteúdo do seu arquivo Program.cs
// ----------------------------------------------------------------------

using minimal_api; // Verifique se o namespace aqui corresponde ao namespace do seu Startup.cs

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                // A linha abaixo é a CHAVE para usar o seu Startup.cs
                webBuilder.UseStartup<Startup>();
            });
}



