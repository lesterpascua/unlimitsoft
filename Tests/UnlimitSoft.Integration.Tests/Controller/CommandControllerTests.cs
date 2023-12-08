using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using UnlimitSoft.WebApi;
using UnlimitSoft.WebApi.Sources.Data;
using Xunit.Abstractions;

namespace UnlimitSoft.Integration.Tests.Controller;


/// <summary>
/// 
/// </summary>
public sealed class CommandControllerTests : IDisposable
{
    private readonly Faker _faker;
    private readonly WebApplicationFactory<Startup> _appFactory;

    public CommandControllerTests(ITestOutputHelper output)
    {
        _faker = new Faker();
        _appFactory = Setup.Factory<Startup, DbContextRead, DbContextWrite>(
        out var apiClient,
            out _,
            services =>
            {
            },
            output: output,
            useInMemContext: true
        );
    }

    public void Dispose()
    {
        _appFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task QueryHandler_AccountProfileWithoutFilter_ShouldOK()
    {
        using var scope = _appFactory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        // Arrange
        var unitOfWork = provider.GetRequiredService<IMyUnitOfWork>();

        await unitOfWork.SaveChangesAsync();
    }
}
