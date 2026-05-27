using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xparf.Core.Abstractions;
using Xparf.Infrastructure.Persistence;
using Xparf.Infrastructure.Services;

namespace Xparf.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("XparfDb")
            ?? "Host=localhost;Port=5432;Database=xparf;Username=postgres;Password=risty1313";

        services.AddScoped<ICurrentUserContext, SystemCurrentUserContext>();
        services.AddDbContext<XparfDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
