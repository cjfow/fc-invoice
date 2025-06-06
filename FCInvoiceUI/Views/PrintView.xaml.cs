﻿using FCInvoiceUI.Models;
using FCInvoiceUI.ViewModels;
using System.Windows;

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
    }
}