using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace RestConsole;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = Program.Services.GetRequiredService<MainWindowViewModel>();

        InitializeComponent();
    }
}