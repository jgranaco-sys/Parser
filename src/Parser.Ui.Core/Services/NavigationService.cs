using Microsoft.Extensions.Logging;
using Parser.Ui.Core.ViewModels;

namespace Parser.Ui.Core.Services;

/// <summary>
/// Simple navigation service that resolves view models via a factory delegate,
/// avoiding a direct dependency on a DI container from Parser.Ui.Core.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly Func<Type, ViewModelBase> _viewModelFactory;
    private readonly ILogger<NavigationService>? _logger;

    public NavigationService(Func<Type, ViewModelBase> viewModelFactory, ILogger<NavigationService>? logger = null)
    {
        _viewModelFactory = viewModelFactory;
        _logger = logger;
    }

    public ViewModelBase? CurrentPage { get; private set; }

    public event EventHandler<ViewModelBase?>? NavigationChanged;

    public void NavigateTo<T>() where T : ViewModelBase
    {
        NavigateTo(typeof(T));
    }

    public void NavigateTo(Type viewModelType)
    {
        var viewModel = _viewModelFactory(viewModelType);
        NavigateTo(viewModel);
    }

    public void NavigateTo(ViewModelBase viewModel)
    {
        CurrentPage = viewModel;
        NavigationChanged?.Invoke(this, CurrentPage);
        _ = InitializeSafelyAsync(viewModel);
    }

    private async Task InitializeSafelyAsync(ViewModelBase viewModel)
    {
        try
        {
            await viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize view model {ViewModelType}.", viewModel.GetType().Name);
        }
    }
}
