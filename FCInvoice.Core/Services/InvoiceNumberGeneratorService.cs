using FCInvoice.Core.Interfaces;
using System.IO;

namespace FCInvoice.Core.Services;

/// <summary>
/// Service responsible for generating sequential invoice numbers in YYYYNNN format
/// </summary>
public class InvoiceNumberGeneratorService : IInvoiceNumberGenerator
{
    private readonly string _folderPath;
    
    public InvoiceNumberGeneratorService()
    {
        _folderPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Data");
    }
    
    public InvoiceNumberGeneratorService(string folderPath)
    {
        _folderPath = folderPath;
    }

    /// <summary>
    /// Generates the next sequential invoice number for the current year
    /// </summary>
    /// <returns>Invoice number in YYYYNNN format</returns>
    public string GetNextInvoiceNumber()
    {
        var currentYear = DateTime.Today.Year;
        
        // Ensure directory exists for invoice storage
        if (!Directory.Exists(_folderPath))
        {
            Directory.CreateDirectory(_folderPath);
            return $"{currentYear}001";
        }

        // Find existing invoice numbers for current year
        var existingNumbers = GetExistingInvoiceNumbers(currentYear);
        var nextNumber = existingNumbers.Count != 0 ? existingNumbers.Max() + 1 : 1;
        
        return $"{currentYear}{nextNumber:D3}";
    }
    
    /// <summary>
    /// Gets all existing invoice sequence numbers for the specified year
    /// </summary>
    /// <param name="year">Year to search for invoice numbers</param>
    /// <returns>List of sequence numbers (001, 002, etc.)</returns>
    private List<int> GetExistingInvoiceNumbers(int year)
    {
        var yearPrefix = year.ToString();
        
        return Directory.GetFiles(_folderPath, "*.enc")
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .Where(name => IsValidInvoiceNumber(name, yearPrefix))
            .Select(name => int.Parse(name[4..]))
            .ToList();
    }
    
    /// <summary>
    /// Validates if a filename represents a valid invoice number for the given year
    /// </summary>
    /// <param name="fileName">Filename without extension</param>
    /// <param name="yearPrefix">Expected year prefix</param>
    /// <returns>True if valid invoice number format</returns>
    private static bool IsValidInvoiceNumber(string? fileName, string yearPrefix) =>
        fileName?.Length == 7 && 
        fileName.StartsWith(yearPrefix) && 
        int.TryParse(fileName[4..], out _);
}