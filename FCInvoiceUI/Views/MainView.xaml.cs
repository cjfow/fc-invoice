using System.Windows;
using FCInvoiceUI.ViewModels;

namespace FCInvoiceUI.Views;

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
