using Parser.Core.Models;
using Parser.Ui.Core.ViewModels;
using Parser.Ui.Infrastructure.Services;
using Xunit;

namespace Parser.Ui.Tests.ViewModels;

public class TemplateEditorViewModelTests
{
    [Fact]
    public async Task SaveTemplateCommand_CreatesNewTemplateWhenNoneSelected()
    {
        var profileService = new InMemoryProfileAppService();
        var templateService = new InMemoryTemplateAppService();
        var profile = await profileService.CreateAsync(new ParserProfile { Name = "Test" });
        await profileService.SetActiveAsync(profile.Id);

        var viewModel = new TemplateEditorViewModel(templateService, profileService);
        await viewModel.InitializeAsync();

        viewModel.TemplateContent = "{ \"value\": {{value}} }";
        await viewModel.SaveTemplateCommand.ExecuteAsync(null);

        Assert.Single(viewModel.Templates);
        Assert.Equal("{ \"value\": {{value}} }", viewModel.SelectedTemplate!.Content);
    }

    [Fact]
    public async Task RenderPreviewAsync_SubstitutesVariables()
    {
        var templateService = new InMemoryTemplateAppService();
        var profileId = "profile-1";
        var template = await templateService.SaveTemplateAsync(profileId, new OutputTemplate
        {
            Name = "Sample",
            Content = "Hello {{name}}!"
        });

        var result = await templateService.RenderPreviewAsync(profileId, template.Id, new Dictionary<string, object>
        {
            ["name"] = "World"
        });

        Assert.Equal("Hello World!", result);
    }
}
