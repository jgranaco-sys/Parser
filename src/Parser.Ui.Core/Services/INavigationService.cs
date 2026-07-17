using Parser.Ui.Core.ViewModels;

namespace Parser.Ui.Core.Services;

public interface INavigationService
{
    ViewModelBase? CurrentPage { get; }

    event EventHandler<ViewModelBase?>? NavigationChanged;

    void NavigateTo<T>() where T : ViewModelBase;

    void NavigateTo(Type viewModelType);

    void NavigateTo(ViewModelBase viewModel);
}
