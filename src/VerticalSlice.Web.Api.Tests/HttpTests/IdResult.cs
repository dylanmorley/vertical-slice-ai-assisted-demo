using System.Text.Json.Serialization;

namespace VerticalSlice.Web.Api.Tests.HttpTests;

public class IdResult
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
}
