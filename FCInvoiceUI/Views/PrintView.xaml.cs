using FCInvoiceUI.Models;
using FCInvoiceUI.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace FCInvoiceUI.Views;

/// <summary>
/// Interaction logic for PrintView.xaml
/// </summary>
public partial class PrintView : Window
{
    public PrintView(BillingInvoice invoice)
    {
        InitializeComponent();
        DataContext = new PrintViewModel(invoice);

        // scale up preview (doesn't affect printed output)
    }
}

