using FCInvoice.Core.Models;
using FCInvoice.UI.ViewModels;
using System.Windows;

namespace FCInvoice.UI.Views;

/// <summary>
/// Interaction logic for PrintView.xaml
/// </summary>
public partial class PrintView : Window
{
    public PrintView(BillingInvoice invoice)
    {
        InitializeComponent();
        DataContext = new PrintViewModel(invoice);
    }
}