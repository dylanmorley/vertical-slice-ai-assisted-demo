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

    public override List<Geography> GetGeographies()
    {
        DateTime now = DateTime.UtcNow;
        List<Geography> geographies = new()
        {
            // Global and regional
            new Geography
            {
                Name = "Global",
                Description = "Global banking operations",
                CountryCode = "GL",
                Region = "Global",
                Longitude = 0.0,
                Latitude = 0.0,
                IsActive = true
            },
            new Geography
            {
                Name = "Americas",
                Description = "North and South American operations",
                CountryCode = "AM",
                Region = "Americas",
                Longitude = -80.0,
                Latitude = 25.0,
                IsActive = true
            },
            new Geography
            {
                Name = "Europe",
                Description = "European operations including UK",
                CountryCode = "EU",
                Region = "EMEA",
                Longitude = 0.0,
                Latitude = 51.0,
                IsActive = true
            },
            new Geography
            {
                Name = "Middle East",
                Description = "Middle East banking operations",
                CountryCode = "ME",
                Region = "EMEA",
                Longitude = 55.0,
                Latitude = 25.0,
                IsActive = true
            },
            new Geography
            {
                Name = "Asia Pacific",
                Description = "Asia Pacific operations",
                CountryCode = "AP",
                Region = "APAC",
                Longitude = 114.0,
                Latitude = 22.0,
                IsActive = true
            },

            // Key financial centers
            new Geography
            {
                Name = "United Kingdom",
                Description = "Group headquarters - London",
                CountryCode = "GB",
                Region = "Europe",
                Longitude = -0.1278,
                Latitude = 51.5074,
                IsActive = true
            },
            new Geography
            {
                Name = "Hong Kong",
                Description = "Asia Pacific headquarters",
                CountryCode = "HK",
                Region = "Asia",
                Longitude = 114.1694,
                Latitude = 22.3193,
                IsActive = true
            },
            new Geography
            {
                Name = "United States",
                Description = "Americas headquarters - New York",
                CountryCode = "US",
                Region = "North America",
                Longitude = -74.006,
                Latitude = 40.7128,
                IsActive = true
            },
            new Geography
            {
                Name = "Singapore",
                Description = "Southeast Asia hub",
                CountryCode = "SG",
                Region = "Asia",
                Longitude = 103.8198,
                Latitude = 1.3521,
                IsActive = true
            },
            new Geography
            {
                Name = "United Arab Emirates",
                Description = "Middle East hub - Dubai",
                CountryCode = "AE",
                Region = "Middle East",
                Longitude = 55.2708,
                Latitude = 25.2048,
                IsActive = true
            },
            new Geography
            {
                Name = "Switzerland",
                Description = "Private banking hub - Geneva",
                CountryCode = "CH",
                Region = "Europe",
                Longitude = 6.1432,
                Latitude = 46.2044,
                IsActive = true
            },
            new Geography
            {
                Name = "China",
                Description = "Mainland China operations",
                CountryCode = "CN",
                Region = "Asia",
                Longitude = 121.4737,
                Latitude = 31.2304,
                IsActive = true
            },
            new Geography
            {
                Name = "India",
                Description = "South Asia operations - Mumbai",
                CountryCode = "IN",
                Region = "Asia",
                Longitude = 72.8777,
                Latitude = 19.076,
                IsActive = true
            },
            new Geography
            {
                Name = "Germany",
                Description = "European Union banking hub - Frankfurt",
                CountryCode = "DE",
                Region = "Europe",
                Longitude = 8.6821,
                Latitude = 50.1109,
                IsActive = true
            },
            new Geography
            {
                Name = "Australia",
                Description = "Oceania operations - Sydney",
                CountryCode = "AU",
                Region = "Oceania",
                Longitude = 151.2093,
                Latitude = -33.8688,
                IsActive = true
            },
            new Geography
            {
                Name = "Mexico",
                Description = "Latin America hub",
                CountryCode = "MX",
                Region = "Americas",
                Longitude = -99.1332,
                Latitude = 19.4326,
                IsActive = true
            },
            new Geography
            {
                Name = "Canada",
                Description = "North American operations - Toronto",
                CountryCode = "CA",
                Region = "North America",
                Longitude = -79.3832,
                Latitude = 43.6532,
                IsActive = true
            }
        };

        foreach (Geography geography in geographies)
        {
            geography.DateCreated = now;
            geography.LastUpdated = now;
        }

        return geographies;
    }
}
