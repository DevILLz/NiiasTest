using System.Windows;

namespace Image;

public partial class MainWindow : Window
{

    public MainWindow() {
        InitializeComponent();
        DataContext = new MainWindowViewModel(StationDrawArea);
    }
}