using System.Diagnostics.CodeAnalysis;

namespace VerticalSlice.Web.Api.Data.SeedDataProviders;

/// <summary>
///     Factory for creating seed data providers based on the selected data set.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SeedDataProviderFactory
{
    private static readonly Dictionary<SeedDataSet, Func<ISeedDataProvider>> Providers = new()
    {
        { SeedDataSet.FinancialInstitution, () => new FinancialInstitutionSeedDataProvider() }
    };

    /// <summary>
    ///     Creates a seed data provider for the specified data set.
    /// </summary>
    /// <param name="dataSet">The data set to create a provider for.</param>
    /// <returns>An instance of the appropriate seed data provider.</returns>
    public static ISeedDataProvider Create(SeedDataSet dataSet)
    {
        if (Providers.TryGetValue(dataSet, out Func<ISeedDataProvider>? factory))
        {
            return factory();
        }

        throw new ArgumentOutOfRangeException(nameof(dataSet), dataSet, "Unknown seed data set");
    }

    /// <summary>
    ///     Creates the default seed data provider (Intelligence Community).
    /// </summary>
    public static ISeedDataProvider CreateDefault() => Create(SeedDataSet.FinancialInstitution);

    /// <summary>
    ///     Parses a seed data set from string (case-insensitive).
    ///     Supports both enum names and numeric values.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="defaultValue">The default value if parsing fails.</param>
    /// <returns>The parsed SeedDataSet or the default value.</returns>
    public static SeedDataSet ParseDataSet(string? value, SeedDataSet defaultValue = SeedDataSet.FinancialInstitution)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        // Try parsing as enum name
        if (Enum.TryParse<SeedDataSet>(value, true, out SeedDataSet result))
        {
            return result;
        }

        // Try parsing as integer
        if (int.TryParse(value, out int numericValue) && Enum.IsDefined(typeof(SeedDataSet), numericValue))
        {
            return (SeedDataSet)numericValue;
        }

        // Handle common aliases
        return value.ToLowerInvariant() switch
        {
            "bank" or "banking" or "finance" or "financial" or "hsbc" => SeedDataSet.FinancialInstitution,
            _ => defaultValue
        };
    }
}
