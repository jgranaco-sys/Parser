using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Parser.Core.Interfaces;
using Parser.Core.Models;

namespace Parser.Ui.Core.ViewModels;

public partial class TemplateEditorViewModel : ViewModelBase
{
    private readonly ITemplateAppService _templateService;
    private readonly IProfileAppService _profileService;
    private CancellationTokenSource? _previewDebounce;

    [ObservableProperty]
    private ObservableCollection<OutputTemplate> _templates = new();

    [ObservableProperty]
    private OutputTemplate? _selectedTemplate;

    [ObservableProperty]
    private string _templateContent = string.Empty;

    [ObservableProperty]
    private string _previewContent = string.Empty;

    [ObservableProperty]
    private string _currentProfileId = string.Empty;

    public TemplateEditorViewModel(ITemplateAppService templateService, IProfileAppService profileService)
    {
        _templateService = templateService;
        _profileService = profileService;
    }

    public override async Task InitializeAsync()
    {
        var profile = await _profileService.GetActiveAsync();
        if (profile == null)
        {
            return;
        }

        CurrentProfileId = profile.Id;
        var templates = await _templateService.GetTemplatesAsync(profile.Id);
        Templates = new ObservableCollection<OutputTemplate>(templates);
        SelectedTemplate = Templates.FirstOrDefault();
        TemplateContent = SelectedTemplate?.Content ?? string.Empty;
    }

    partial void OnTemplateContentChanged(string value)
    {
        _ = DebouncePreviewAsync(value);
    }

    private async Task DebouncePreviewAsync(string content)
    {
        _previewDebounce?.Cancel();
        var cts = new CancellationTokenSource();
        _previewDebounce = cts;

        try
        {
            await Task.Delay(500, cts.Token);
            if (SelectedTemplate != null)
            {
                var variables = new Dictionary<string, object>
                {
                    ["profileId"] = CurrentProfileId,
                    ["timestamp"] = DateTimeOffset.UtcNow.ToString("O")
                };
                PreviewContent = await _templateService.RenderPreviewAsync(CurrentProfileId, SelectedTemplate.Id, variables);
            }
            else
            {
                PreviewContent = content;
            }
        }
        catch (OperationCanceledException)
        {
            // A newer edit superseded this preview render.
        }
    }

    [RelayCommand]
    private async Task SaveTemplateAsync()
    {
        if (string.IsNullOrEmpty(CurrentProfileId))
        {
            return;
        }

        var template = SelectedTemplate ?? new OutputTemplate { Name = "New Template", Format = OutputFormat.JSON };
        template.Content = TemplateContent;

        var saved = await _templateService.SaveTemplateAsync(CurrentProfileId, template);
        if (!Templates.Contains(saved))
        {
            Templates.Add(saved);
        }
        SelectedTemplate = saved;
    }
}
