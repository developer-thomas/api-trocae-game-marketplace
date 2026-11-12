using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trocae.Infrastructure.Identity;
using Trocae.Infrastructure.Identity.CpfVerification;
using Trocae.Infrastructure.Persistence;

namespace Trocae.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTrocaeInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TrocaeDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("TrocaeDb")));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<TrocaeDbContext>();

        services.AddHttpClient<ICpfHubClient, CpfHubClient>();
        services.AddScoped<ICpfVerificationService, CpfVerificationService>();

        return services;
    }
}
