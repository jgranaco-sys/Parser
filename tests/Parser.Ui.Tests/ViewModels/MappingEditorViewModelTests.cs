using Parser.Core.Models;
using Parser.Ui.Core.ViewModels;
using Parser.Ui.Infrastructure.Services;
using Xunit;

namespace Parser.Ui.Tests.ViewModels;

public class MappingEditorViewModelTests
{
    private static async Task<(InMemoryProfileAppService profiles, InMemoryMappingAppService mappings, ParserProfile profile)> SetupAsync()
    {
        var profileService = new InMemoryProfileAppService();
        var mappingService = new InMemoryMappingAppService();
        var profile = await profileService.CreateAsync(new ParserProfile { Name = "Test" });
        await profileService.SetActiveAsync(profile.Id);
        return (profileService, mappingService, profile);
    }

    [Fact]
    public async Task InitializeAsync_LoadsMappingsForActiveProfile()
    {
        var (profileService, mappingService, profile) = await SetupAsync();
        await mappingService.AddMappingAsync(profile.Id, new FieldMapping { IncomingField = "temp", ScadaVariable = "Temp" });

        var viewModel = new MappingEditorViewModel(mappingService, profileService);
        await viewModel.InitializeAsync();

        Assert.Single(viewModel.Mappings);
        Assert.Equal(profile.Id, viewModel.CurrentProfileId);
    }

    [Fact]
    public async Task AddMappingCommand_AddsNewMappingToCollection()
    {
        var (profileService, mappingService, _) = await SetupAsync();
        var viewModel = new MappingEditorViewModel(mappingService, profileService);
        await viewModel.InitializeAsync();

        await viewModel.AddMappingCommand.ExecuteAsync(null);

        Assert.Single(viewModel.Mappings);
        Assert.NotNull(viewModel.SelectedMapping);
    }

    [Fact]
    public async Task DeleteMappingCommand_RemovesSelectedMapping()
    {
        var (profileService, mappingService, profile) = await SetupAsync();
        var mapping = await mappingService.AddMappingAsync(profile.Id, new FieldMapping { IncomingField = "temp" });

        var viewModel = new MappingEditorViewModel(mappingService, profileService);
        await viewModel.InitializeAsync();
        viewModel.SelectedMapping = viewModel.Mappings.First(m => m.Id == mapping.Id);

        await viewModel.DeleteMappingCommand.ExecuteAsync(null);

        Assert.Empty(viewModel.Mappings);
    }

    [Fact]
    public async Task UpdateMappingCommand_PersistsChanges()
    {
        var (profileService, mappingService, profile) = await SetupAsync();
        var mapping = await mappingService.AddMappingAsync(profile.Id, new FieldMapping { IncomingField = "temp" });

        var viewModel = new MappingEditorViewModel(mappingService, profileService);
        await viewModel.InitializeAsync();
        var toUpdate = viewModel.Mappings.First(m => m.Id == mapping.Id);
        toUpdate.ScadaVariable = "Weather.Temp";

        await viewModel.UpdateMappingCommand.ExecuteAsync(toUpdate);

        var mappings = await mappingService.GetMappingsAsync(profile.Id);
        Assert.Equal("Weather.Temp", mappings.First(m => m.Id == mapping.Id).ScadaVariable);
    }
}
