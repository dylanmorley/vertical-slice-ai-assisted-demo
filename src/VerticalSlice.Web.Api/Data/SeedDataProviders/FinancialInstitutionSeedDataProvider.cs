using System.Diagnostics.CodeAnalysis;
using VerticalSlice.Web.Api.Contracts;
using VerticalSlice.Web.Api.Model;

namespace VerticalSlice.Web.Api.Data.SeedDataProviders;

/// <summary>
///     Seeds the database with global financial institution data similar to HSBC.
///     Models a multinational bank with retail, commercial, and investment banking across multiple geographies.
/// </summary>
[ExcludeFromCodeCoverage]
public class FinancialInstitutionSeedDataProvider : SeedDataProviderBase
{
    public override SeedDataSet DataSet => SeedDataSet.FinancialInstitution;

}
