namespace VerticalSlice.Web.Api.Model;

public class Geography : AuditedEntity
{
    public int GeographyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public double Longitude { get; set; }

    public double Latitude { get; set; }

    public bool IsActive { get; set; } = true;
}
