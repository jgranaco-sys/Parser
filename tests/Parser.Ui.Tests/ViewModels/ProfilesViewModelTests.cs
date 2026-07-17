using Parser.Core.Models;
using Parser.Ui.Core.ViewModels;
using Parser.Ui.Infrastructure.Services;
using Xunit;

namespace Parser.Ui.Tests.ViewModels;

public class ProfilesViewModelTests
{
    [Fact]
    public async Task CreateCommand_AddsNewProfile()
    {
        var profileService = new InMemoryProfileAppService();
        var viewModel = new ProfilesViewModel(profileService);
        await viewModel.InitializeAsync();

        viewModel.NewProfileName = "My Profile";
        await viewModel.CreateCommand.ExecuteAsync(null);

        Assert.Contains(viewModel.Profiles, p => p.Name == "My Profile");
        Assert.Equal(string.Empty, viewModel.NewProfileName);
    }

    [Fact]
    public async Task DuplicateCommand_ClonesSelectedProfile()
    {
        var profileService = new InMemoryProfileAppService();
        var original = await profileService.CreateAsync(new ParserProfile { Name = "Original" });

        var viewModel = new ProfilesViewModel(profileService);
        await viewModel.InitializeAsync();
        viewModel.SelectedProfile = viewModel.Profiles.First(p => p.Id == original.Id);

        await viewModel.DuplicateCommand.ExecuteAsync(null);

        Assert.Contains(viewModel.Profiles, p => p.Name == "Original (Copy)");
    }

    [Fact]
    public async Task DeleteCommand_RemovesSelectedProfile()
    {
        var profileService = new InMemoryProfileAppService();
        var profile = await profileService.CreateAsync(new ParserProfile { Name = "ToDelete" });

        var viewModel = new ProfilesViewModel(profileService);
        await viewModel.InitializeAsync();
        viewModel.SelectedProfile = viewModel.Profiles.First(p => p.Id == profile.Id);

        await viewModel.DeleteCommand.ExecuteAsync(null);

        Assert.DoesNotContain(viewModel.Profiles, p => p.Id == profile.Id);
    }

    [Fact]
    public async Task SetActiveCommand_MarksSelectedProfileActive()
    {
        var profileService = new InMemoryProfileAppService();
        var profileA = await profileService.CreateAsync(new ParserProfile { Name = "A" });
        var profileB = await profileService.CreateAsync(new ParserProfile { Name = "B" });

        var viewModel = new ProfilesViewModel(profileService);
        await viewModel.InitializeAsync();
        viewModel.SelectedProfile = viewModel.Profiles.First(p => p.Id == profileB.Id);

        await viewModel.SetActiveCommand.ExecuteAsync(null);

        Assert.True(viewModel.Profiles.First(p => p.Id == profileB.Id).IsActive);
        Assert.False(viewModel.Profiles.First(p => p.Id == profileA.Id).IsActive);
    }

    [Fact]
    public async Task UpdateCommand_PersistsChangesToSelectedProfile()
    {
        var profileService = new InMemoryProfileAppService();
        var profile = await profileService.CreateAsync(new ParserProfile { Name = "Old Name" });

        var viewModel = new ProfilesViewModel(profileService);
        await viewModel.InitializeAsync();
        viewModel.SelectedProfile = viewModel.Profiles.First(p => p.Id == profile.Id);
        viewModel.SelectedProfile!.Name = "New Name";

        await viewModel.UpdateCommand.ExecuteAsync(null);

        var updated = await profileService.GetByIdAsync(profile.Id);
        Assert.Equal("New Name", updated!.Name);
    }
}
