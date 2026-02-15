using VerticalSlice.Web.Api.Model;

namespace VerticalSlice.Web.Api.Data.SeedDataProviders;

/// <summary>
///     Contract for providing seed data for database initialization.
///     Each implementation provides a complete data set for a specific domain/industry.
/// </summary>
public interface ISeedDataProvider
{
    /// <summary>
    ///     The identifier for this seed data set.
    /// </summary>
    SeedDataSet DataSet { get; }

    /// <summary>
    ///     Provides geographic locations for risk mapping.
    /// </summary>
    List<Geography> GetGeographies();
}
