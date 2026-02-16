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

}
