using System;
using System.Collections.ObjectModel;
using FCInvoiceUI.Models;

namespace FCInvoiceUI.ViewModels;

public class PrintViewModel(BillingInvoice invoice)
{
    public string? BillTo { get; } = invoice.BillTo;
    public string? ProjectNumber { get; } = invoice.ProjectNumber;
    public string? InvoiceNumber { get; } = invoice.InvoiceNumber;
    public DateTime SelectedDate { get; } = invoice.SelectedDate;
    public ObservableCollection<InvoiceItem> InvoiceItems { get; } = invoice.Items;
    public decimal Total { get; } = invoice.Total;
}
