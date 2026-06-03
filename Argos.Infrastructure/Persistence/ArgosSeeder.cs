using Argos.Domain.Entities;
using Argos.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Argos.Infrastructure.Persistence;

/// <summary>
/// Seeds mínimos do Argos : tipos de ocorrência, uma conta da Defesa Civil
/// e as zonas de risco de referência. Idempotente — só insere o que ainda falta,
/// então pode rodar em todo start sem duplicar nada.
/// </summary>
public static class ArgosSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArgosContext>();

        await SeedTiposOcorrenciaAsync(context);
        await SeedDefesaCivilAsync(context);
        await SeedZonasRiscoAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedTiposOcorrenciaAsync(ArgosContext context)
    {
        var tipos = new (string Nome, string Chave)[]
        {
            ("Alagamento", "alagamento"),
            ("Deslizamento", "deslizamento"),
            ("Outro", "outro"),
        };

        var chavesExistentes = await context.TiposOcorrencia
            .Select(t => t.Chave)
            .ToListAsync();

        foreach (var (nome, chave) in tipos)
        {
            if (!chavesExistentes.Contains(chave))
                context.TiposOcorrencia.Add(new TipoOcorrencia(nome, chave));
        }
    }

    private static async Task SeedDefesaCivilAsync(ArgosContext context)
    {
        const string email = "defesacivil@argos.gov.br";

        var jaExiste = await context.Usuarios.AnyAsync(u => u.Email == email);
        if (jaExiste)
            return;

        context.Usuarios.Add(new Usuario(
            nome: "Defesa Civil",
            email: email,
            senha: "argos123",
            telefone: "199",
            tipoUsuario: TipoUsuario.DEFESA_CIVIL));
    }

    private static async Task SeedZonasRiscoAsync(ArgosContext context)
    {
        var zonas = new (string Nome, string Regiao, double Lat, double Lng)[]
        {
            ("Casa Verde", "Norte", -23.5089, -46.6589),
            ("Jardim Ângela", "Sul", -23.6917, -46.7642),
            ("Vila Prudente", "Leste", -23.5847, -46.5806),
            ("Pinheiros", "Oeste", -23.5614, -46.7019),
            ("Sé", "Central", -23.5505, -46.6333),
        };

        var nomesExistentes = await context.ZonasRisco
            .Select(z => z.Nome)
            .ToListAsync();

        foreach (var (nome, regiao, lat, lng) in zonas)
        {
            if (!nomesExistentes.Contains(nome))
            {
                context.ZonasRisco.Add(new ZonaRisco(
                    nome: nome,
                    cidade: "São Paulo",
                    estado: "SP",
                    latitude: lat,
                    longitude: lng,
                    regiao: regiao));
            }
        }
    }
}
