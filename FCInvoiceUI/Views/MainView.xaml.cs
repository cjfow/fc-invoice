using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
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

    // a simple regex pattern to allow digits, decimal separator, and group separator
    // note: we get these from current culture:
    // the pattern may need tweaking based on your exact culture.
    private readonly string allowedPattern = @"^[0-9" +
        Regex.Escape(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) +
        Regex.Escape(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator) +
        "]+$";

    private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // if the input text does not match the allowed pattern, mark it handled.
        if (!Regex.IsMatch(e.Text, allowedPattern))
        {
            e.Handled = true;
        }
    }

    // handle pasting data
    private void NumericOnly_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(typeof(string)))
        {
            e.CancelCommand();
            return;
        }
        string pasteText = (string)e.DataObject.GetData(typeof(string));
        e.CancelCommand();
    }
}
