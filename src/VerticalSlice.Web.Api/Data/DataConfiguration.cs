using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace VerticalSlice.Web.Api.Data;

[ExcludeFromCodeCoverage]
public static class DataConfiguration
{
    public static void AddEntityFramework(this IServiceCollection services, SqlProviderType providerType, string? sqlConnectionString)
    {
        services.AddDbContext<VerticalSliceDataContext>((sp, options) =>
        {
            switch (providerType)
            {
                case SqlProviderType.SqlLite:
                    options.UseSqlite(sqlConnectionString, builder =>
                        builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    break;

                case SqlProviderType.MySql:
                    options.UseMySql(
                        sqlConnectionString,
                        ServerVersion.AutoDetect(sqlConnectionString), builder =>
                            builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    break;

                case SqlProviderType.SqlServer:
                    options.UseSqlServer(sqlConnectionString, builder =>
                        builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    break;

                case SqlProviderType.PostgreSql:
                    options.UseNpgsql(sqlConnectionString, builder =>
                        builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(providerType), providerType, null);
            }
        });
    }


    public enum SqlProviderType
    {
        SqlLite,
        MySql,
        SqlServer,
        PostgreSql,
    }
}
