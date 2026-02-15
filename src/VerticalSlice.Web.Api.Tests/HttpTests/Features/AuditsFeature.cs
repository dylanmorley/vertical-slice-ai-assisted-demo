using System.Net;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.NUnit3;
using VerticalSlice.Web.Api.Tests.HttpTests.ClientApplication;

namespace VerticalSlice.Web.Api.Tests.HttpTests.Features;

[FeatureDescription(
    @"I want an API for accessing audit information
        So that I can track changes and activities in the system")]
public class AuditsFeature : FeatureFixture
{
    [Scenario(Description = "Get audit information")]
    public async Task GetsAuditData()
    {
        await Runner
            .WithContext<AuditsContext>()
            .AddAsyncSteps(
                given => given.The_route_is_requested("api/v1/audit"),
                then => then.The_response_code_should_be(HttpStatusCode.OK),
                and => and.The_response_should_contain_audit_data()
            ).RunAsync();
    }
}
