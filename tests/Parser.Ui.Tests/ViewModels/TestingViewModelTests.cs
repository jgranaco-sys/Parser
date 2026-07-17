using Parser.Core.Models;
using Parser.Ui.Core.ViewModels;
using Parser.Ui.Infrastructure.Services;
using Xunit;

namespace Parser.Ui.Tests.ViewModels;

public class TestingViewModelTests
{
    [Fact]
    public async Task RunTestCommand_ProducesResultForSamplePayload()
    {
        var profileService = new InMemoryProfileAppService();
        var mappingService = new InMemoryMappingAppService();
        var testRunService = new InMemoryTestRunAppService(mappingService);

        var profile = await profileService.CreateAsync(new ParserProfile { Name = "Test", InputFormat = "JSON" });
        await profileService.SetActiveAsync(profile.Id);
        await mappingService.AddMappingAsync(profile.Id, new FieldMapping { IncomingField = "temperature", ScadaVariable = "Weather.Temperature" });

        var viewModel = new TestingViewModel(testRunService, profileService);
        await viewModel.InitializeAsync();
        viewModel.PayloadInput = "{\"temperature\": 21.5}";

        await viewModel.RunTestCommand.ExecuteAsync(null);

        Assert.NotNull(viewModel.Result);
        Assert.True(viewModel.Result!.Success);
        Assert.Contains("Weather.Temperature", viewModel.Result.MappedValues.Keys);
        Assert.False(viewModel.IsRunning);
    }

    [Fact]
    public async Task RunTestCommand_ReportsErrorsForMissingFields()
    {
        var profileService = new InMemoryProfileAppService();
        var mappingService = new InMemoryMappingAppService();
        var testRunService = new InMemoryTestRunAppService(mappingService);

        var profile = await profileService.CreateAsync(new ParserProfile { Name = "Test", InputFormat = "JSON" });
        await profileService.SetActiveAsync(profile.Id);
        await mappingService.AddMappingAsync(profile.Id, new FieldMapping { IncomingField = "missingField", ScadaVariable = "X" });

        var viewModel = new TestingViewModel(testRunService, profileService);
        await viewModel.InitializeAsync();
        viewModel.PayloadInput = "{}";

        await viewModel.RunTestCommand.ExecuteAsync(null);

        Assert.False(viewModel.Result!.Success);
        Assert.NotEmpty(viewModel.Result.Errors);
    }
}
