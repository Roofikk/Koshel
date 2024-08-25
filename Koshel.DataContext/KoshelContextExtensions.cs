using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koshel.DataContext;

public static class KoshelContextExtensions
{
    public static IServiceCollection AddKoshelContext(this IServiceCollection services, string? connectionString)
    {
        if (connectionString is null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        return services.AddDbContext<KoshelContext>(opts =>
        {
            opts.UseNpgsql(connectionString, npgOpts =>
            {
                npgOpts.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
        });
    }

    public static async Task InitializeDatabaseAsync(this KoshelContext context)
    {
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }
    }
}
