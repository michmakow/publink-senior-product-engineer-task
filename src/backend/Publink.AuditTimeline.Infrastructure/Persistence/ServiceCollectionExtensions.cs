using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Publink.AuditTimeline.Application.ContractsAudit;
using Publink.AuditTimeline.Infrastructure.Repositories;

namespace Publink.AuditTimeline.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditTimelineInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AuditDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IAuditLogRepository, EfAuditLogRepository>();
        services.AddScoped<AuditDbInitializer>();

        return services;
    }
}
