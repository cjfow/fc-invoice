using FCInvoice.Core.Interfaces;

namespace FCInvoiceTests.Mocks;

/// <summary>
/// Mock implementation of IInvoiceNumberGenerator for testing
/// </summary>
public class MockInvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private int _nextNumber = 1;
    private readonly int _year;

    public MockInvoiceNumberGenerator() : this(DateTime.Today.Year) { }
    
    public MockInvoiceNumberGenerator(int year)
    {
        _year = year;
    }

    /// <summary>
    /// Generates the next sequential test invoice number
    /// </summary>
    /// <returns>Invoice number in YYYYNNN format</returns>
    public string GetNextInvoiceNumber()
    {
        var number = $"{_year}{_nextNumber:D3}";
        _nextNumber++;
        return number;
    }

    /// <summary>
    /// Sets the next number to be generated (for test control)
    /// </summary>
    /// <param name="nextNumber">Next number to generate</param>
    public void SetNextNumber(int nextNumber)
    {
        _nextNumber = nextNumber;
    }

    /// <summary>
    /// Resets the generator to start from 1
    /// </summary>
    public void Reset()
    {
        _nextNumber = 1;
    }
}