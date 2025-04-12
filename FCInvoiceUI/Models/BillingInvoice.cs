
namespace FCInvoiceUI.Models;

/// <summary>
/// Represents a customer invoice with header information and a list of invoice items.
/// </summary>
public class BillingInvoice
{
    public DateTime SelectedDate { get; set; } = DateTime.Today;

    public uint InvoiceNumber { get; set; }

    public string? ProjectNumber { get; set; }

    public string? BillTo { get; set; }

    public List<InvoiceItem> Items { get; set; } = [];

    public decimal Total => Items.Sum(i => i.Amount);
}
