using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace VerticalSlice.Web.Api.Tests.HttpTests.ClientApplication;

public abstract class ContextBase(HttpClient httpClient)
{
    protected HttpClient HttpClient { get; } = httpClient;

    protected HttpResponseMessage? Response;

    protected string ResponseContent = string.Empty;

    public async Task The_route_is_requested(string route) => await The_route_is_requested(route, "application/json");

    public async Task The_route_is_requested(string route, string contentType)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, route);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

        Response = await HttpClient.SendAsync(message);
    }

    public async Task The_response_code_should_be(HttpStatusCode statusCode)
    {
        if (Response != null && Response.StatusCode != statusCode)
        {
            var content = await Response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
        }

        Assert.That(Response!.StatusCode, Is.EqualTo(statusCode));
    }

    public Task The_response_content_should_be(string expectedContentType)
    {
        Assert.That(Response!.Content.Headers.ContentType?.MediaType, Is.EqualTo(expectedContentType));
        return Task.CompletedTask;
    }

    public async Task The_response_content_is_read()
    {
        ResponseContent = await Response?.Content.ReadAsStringAsync()!;
    }
}
