using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Parser.Ui.Controls.Controls;

public partial class PayloadViewerControl : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<PayloadViewerControl, string?>(nameof(Text));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public PayloadViewerControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
