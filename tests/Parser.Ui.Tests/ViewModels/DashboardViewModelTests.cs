using Parser.Core.Models;
using Parser.Ui.Core.ViewModels;
using Parser.Ui.Infrastructure.Services;
using Xunit;

namespace Parser.Ui.Tests.ViewModels;

public class DashboardViewModelTests
{
    [Fact]
    public async Task InitializeAsync_PopulatesActiveProfileAndStatus()
    {
        var profileService = new InMemoryProfileAppService();
        var diagnosticsService = new InMemoryDiagnosticsAppService();

        var profile = await profileService.CreateAsync(new ParserProfile { Name = "Test Profile" });
        await profileService.SetActiveAsync(profile.Id);

        var viewModel = new DashboardViewModel(diagnosticsService, profileService);

        await viewModel.InitializeAsync();

        Assert.Equal("Test Profile", viewModel.ActiveProfileName);
        Assert.False(string.IsNullOrEmpty(viewModel.Status));
    }

    [Fact]
    public async Task InitializeAsync_WithNoActiveProfile_ShowsNone()
    {
        var profileService = new InMemoryProfileAppService();
        var diagnosticsService = new InMemoryDiagnosticsAppService();

        var viewModel = new DashboardViewModel(diagnosticsService, profileService);

        await viewModel.InitializeAsync();

        Assert.Equal("None", viewModel.ActiveProfileName);
    }
}
