using Parser.Core.Models;
using Parser.Ui.Core.ViewModels;
using Parser.Ui.Infrastructure.Services;
using Xunit;

namespace Parser.Ui.Tests.ViewModels;

public class SettingsViewModelTests
{
    [Fact]
    public void ToggleThemeCommand_SwitchesBetweenDarkAndLight()
    {
        var settingsService = new InMemorySettingsAppService();
        var viewModel = new SettingsViewModel(settingsService);

        Assert.Equal(AppTheme.Dark, viewModel.Theme);

        viewModel.ToggleThemeCommand.Execute(null);
        Assert.Equal(AppTheme.Light, viewModel.Theme);

        viewModel.ToggleThemeCommand.Execute(null);
        Assert.Equal(AppTheme.Dark, viewModel.Theme);
    }

    [Fact]
    public async Task SaveCommand_PersistsSettings()
    {
        var settingsService = new InMemorySettingsAppService();
        var viewModel = new SettingsViewModel(settingsService);

        viewModel.Language = "de";
        viewModel.Autosave = false;
        await viewModel.SaveCommand.ExecuteAsync(null);

        var saved = await settingsService.GetSettingsAsync();
        Assert.Equal("de", saved.Language);
        Assert.False(saved.Autosave);
        Assert.True(viewModel.IsSaved);
    }

    [Fact]
    public async Task InitializeAsync_LoadsExistingSettings()
    {
        var settingsService = new InMemorySettingsAppService();
        await settingsService.SaveSettingsAsync(new AppSettings { Theme = AppTheme.Light, Language = "fr" });

        var viewModel = new SettingsViewModel(settingsService);
        await viewModel.InitializeAsync();

        Assert.Equal(AppTheme.Light, viewModel.Theme);
        Assert.Equal("fr", viewModel.Language);
    }
}
