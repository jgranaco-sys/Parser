using CommunityToolkit.Mvvm.ComponentModel;

namespace Parser.Ui.Core.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    public virtual string Title => GetType().Name.Replace("ViewModel", string.Empty);

    public virtual Task InitializeAsync() => Task.CompletedTask;
}
