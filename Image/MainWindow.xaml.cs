using System.Windows;

namespace WPF.StationViewTest;

public partial class MainWindow : Window
{

    public MainWindow() {
        InitializeComponent();
        DataContext = new MainWindowViewModel(StationDrawArea);
    }
}