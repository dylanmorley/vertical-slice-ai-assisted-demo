namespace VerticalSlice.Web.Api.Tests.HttpTests.ClientApplication;

public class AuditsContext(HttpClient httpClient) : ContextBase(httpClient)
{
    public Task The_response_should_contain_audit_data()
    {
        Assert.That(Response, Is.Not.Null);
        Assert.That(Response!.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
        return Task.CompletedTask;
    }
}
