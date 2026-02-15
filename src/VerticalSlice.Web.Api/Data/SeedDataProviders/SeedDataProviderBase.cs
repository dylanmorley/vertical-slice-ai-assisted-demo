using System.Diagnostics.CodeAnalysis;
using VerticalSlice.Web.Api.Contracts;
using VerticalSlice.Web.Api.Model;

namespace VerticalSlice.Web.Api.Data.SeedDataProviders;

/// <summary>
///     Base class for seed data providers with shared helper methods.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class SeedDataProviderBase : ISeedDataProvider
{
    public abstract SeedDataSet DataSet { get; }

    public abstract List<Geography> GetGeographies();

    protected static string GenerateSlug(string name) =>
        name.ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace(",", "")
            .Replace(".", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("&", "and");

    protected static Geography Geo(List<Geography> geographies, string name) =>
        geographies.First(g => g.Name == name);
}
