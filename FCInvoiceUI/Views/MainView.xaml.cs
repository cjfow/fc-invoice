using System.Windows;
using FCInvoice.UI.ViewModels;

namespace FCInvoice.UI.Views;

/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
