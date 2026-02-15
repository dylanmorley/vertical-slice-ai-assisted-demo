using System.Reflection;
using LightBDD.Core.Configuration;
using LightBDD.Extensions.DependencyInjection;
using LightBDD.NUnit3;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using VerticalSlice.Web.Api.Tests.HttpTests;
using VerticalSlice.Web.Api.Tests.HttpTests.ClientApplication;

[assembly: Parallelizable(ParallelScope.None)]
[assembly: ConfiguredLightBddScope]

namespace VerticalSlice.Web.Api.Tests.HttpTests;

public class ConfiguredLightBddScopeAttribute : LightBddScopeAttribute
{
    private ServiceProvider? _serviceProvider;

    protected override void OnConfigure(LightBddConfiguration configuration)
    {
        try
        {
            _serviceProvider = ConfigureDi();
            configuration
                .DependencyContainerConfiguration()
                .UseContainer(_serviceProvider, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    protected override void OnSetUp()
    {
        if (_serviceProvider != null)
        {
            var application = _serviceProvider.GetRequiredService<VerticalSliceApplication>();
            application.ResetDatabaseAsync().GetAwaiter().GetResult();
        }
    }

    protected override void OnTearDown()
    {
        _serviceProvider?.Dispose();
    }

    private ServiceProvider ConfigureDi()
    {
        var services = new ServiceCollection();

        // Use Singleton for performance but with database cleanup between tests
        services.AddSingleton<VerticalSliceApplication>();
        services.AddSingleton(provider =>
        {
            var application = provider.GetRequiredService<VerticalSliceApplication>();
            var client = application.CreateClient(new WebApplicationFactoryClientOptions()
            {
                BaseAddress = new Uri(VerticalSliceApplication.BaseUrl)
            });
            return client;
        });

        var derivedTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(ContextBase)));

        foreach (var derivedType in derivedTypes)
        {
            services.AddScoped(derivedType);
        }

        return services.BuildServiceProvider();
    }
}
