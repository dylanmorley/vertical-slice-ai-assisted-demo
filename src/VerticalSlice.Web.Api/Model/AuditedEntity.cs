namespace VerticalSlice.Web.Api.Model;

public abstract class AuditedEntity
{
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public int Age => (DateTime.UtcNow - DateCreated).Days;

    public string CreatedBy { get; set; } = "system";

    public string UpdatedBy { get; set; } = "system";
}
