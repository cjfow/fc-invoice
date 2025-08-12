namespace FCInvoice.Core.Interfaces;

/// <summary>
/// Interface for generating sequential invoice numbers
/// </summary>
public interface IInvoiceNumberGenerator
{
    /// <summary>
    /// Generates the next sequential invoice number for the current year
    /// </summary>
    /// <returns>Invoice number in YYYYNNN format</returns>
    string GetNextInvoiceNumber();
}