using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Parser.Ui.Controls.Controls;

public partial class DiagnosticsPanelControl : UserControl
{
    public static readonly StyledProperty<System.Collections.IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<DiagnosticsPanelControl, System.Collections.IEnumerable?>(nameof(ItemsSource));

    public System.Collections.IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public DiagnosticsPanelControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
