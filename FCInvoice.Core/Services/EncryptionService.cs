using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FCInvoice.Core.Services;

/// <summary>
/// Service for encrypting and decrypting invoice data using AES-256 with DPAPI key protection
/// </summary>
public class EncryptionService
{
    private readonly string _keyFilePath;

    public EncryptionService()
    {
        _keyFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Data", "KeyIV.dat");
        EnsureKeyFileExists();
    }
    
    /// <summary>
    /// Ensures the encryption key file exists, creating it if necessary
    /// </summary>
    private void EnsureKeyFileExists()
    {
        if (!File.Exists(_keyFilePath))
        {
            var (key, iv) = GenerateKeyAndIV();
            SaveKeyAndIV(key, iv, _keyFilePath);
        }
    }

    /// <summary>
    /// Encrypts a file using AES-256 encryption
    /// </summary>
    /// <param name="inputFile">Path to the file to encrypt</param>
    /// <param name="outputFile">Path where the encrypted file will be saved</param>
    /// <exception cref="ArgumentException">Thrown when file paths are invalid</exception>
    /// <exception cref="FileNotFoundException">Thrown when input file doesn't exist</exception>
    public void EncryptFile(string inputFile, string outputFile)
    {
        if (string.IsNullOrWhiteSpace(inputFile))
            throw new ArgumentException("Input file path cannot be null or empty", nameof(inputFile));
        if (string.IsNullOrWhiteSpace(outputFile))
            throw new ArgumentException("Output file path cannot be null or empty", nameof(outputFile));
        if (!File.Exists(inputFile))
            throw new FileNotFoundException($"Input file not found: {inputFile}");

        var (key, iv) = LoadKeyAndIV();

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using FileStream fsInput = new(inputFile, FileMode.Open, FileAccess.Read);
        using FileStream fsEncrypted = new(outputFile, FileMode.Create, FileAccess.Write);
        using CryptoStream cs = new(fsEncrypted, aes.CreateEncryptor(), CryptoStreamMode.Write);

        fsInput.CopyTo(cs);
    }

    /// <summary>
    /// Decrypts a file that was encrypted using AES-256 encryption
    /// </summary>
    /// <param name="inputFile">Path to the encrypted file</param>
    /// <param name="outputFile">Path where the decrypted file will be saved</param>
    /// <exception cref="ArgumentException">Thrown when file paths are invalid</exception>
    /// <exception cref="FileNotFoundException">Thrown when input file doesn't exist</exception>
    public void DecryptFile(string inputFile, string outputFile)
    {
        if (string.IsNullOrWhiteSpace(inputFile))
            throw new ArgumentException("Input file path cannot be null or empty", nameof(inputFile));
        if (string.IsNullOrWhiteSpace(outputFile))
            throw new ArgumentException("Output file path cannot be null or empty", nameof(outputFile));
        if (!File.Exists(inputFile))
            throw new FileNotFoundException($"Input file not found: {inputFile}");

        var (key, iv) = LoadKeyAndIV();

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using FileStream fsEncrypted = new(inputFile, FileMode.Open, FileAccess.Read);
        using FileStream fsDecrypted = new(outputFile, FileMode.Create, FileAccess.Write);
        using CryptoStream cs = new(fsEncrypted, aes.CreateDecryptor(), CryptoStreamMode.Read);

        cs.CopyTo(fsDecrypted);
    }

    /// <summary>
    /// Encrypts a string using AES-256 encryption and returns a Base64-encoded result
    /// </summary>
    /// <param name="plainText">Text to encrypt</param>
    /// <returns>Base64-encoded encrypted text</returns>
    /// <exception cref="ArgumentException">Thrown when plainText is null or empty</exception>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));

        try
        {
            var (key, iv) = LoadKeyAndIV();

            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (StreamWriter sw = new(cs, Encoding.UTF8))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Encryption failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Decrypts a Base64-encoded string that was encrypted using AES-256
    /// </summary>
    /// <param name="cipherText">Base64-encoded encrypted text</param>
    /// <returns>Decrypted plain text</returns>
    /// <exception cref="ArgumentException">Thrown when cipherText is null or empty</exception>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("Cipher text cannot be null or empty", nameof(cipherText));

        try
        {
            var (key, iv) = LoadKeyAndIV();
            var buffer = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using MemoryStream ms = new(buffer);
            using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader sr = new(cs, Encoding.UTF8);
            return sr.ReadToEnd();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Decryption failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Generates a new AES-256 key and initialization vector
    /// </summary>
    /// <returns>Tuple containing the generated key and IV</returns>
    private static (byte[] Key, byte[] IV) GenerateKeyAndIV()
    {
        using Aes aes = Aes.Create();
        aes.KeySize = 256; // Use AES-256 for maximum security
        aes.GenerateKey();
        aes.GenerateIV();
        return (aes.Key, aes.IV);
    }

    /// <summary>
    /// Saves the encryption key and IV to a file protected by Windows DPAPI
    /// </summary>
    /// <param name="key">AES encryption key</param>
    /// <param name="iv">AES initialization vector</param>
    /// <param name="filePath">Path where the protected key file will be saved</param>
    private static void SaveKeyAndIV(byte[] key, byte[] iv, string filePath)
    {
        // Combine key and IV into a single byte array
        var combined = new byte[key.Length + iv.Length];
        Buffer.BlockCopy(key, 0, combined, 0, key.Length);
        Buffer.BlockCopy(iv, 0, combined, key.Length, iv.Length);

        // Protect the combined key/IV using Windows DPAPI (current user scope)
        var encrypted = ProtectedData.Protect(combined, null, DataProtectionScope.CurrentUser);
        
        // Ensure directory exists before writing
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        File.WriteAllBytes(filePath, encrypted);
    }

    /// <summary>
    /// Loads and unprotects the encryption key and IV from the protected file
    /// </summary>
    /// <returns>Tuple containing the key and IV</returns>
    /// <exception cref="FileNotFoundException">Thrown when key file doesn't exist</exception>
    private (byte[] Key, byte[] IV) LoadKeyAndIV()
    {
        if (!File.Exists(_keyFilePath))
            throw new FileNotFoundException($"Encryption key file not found: {_keyFilePath}");

        var encrypted = File.ReadAllBytes(_keyFilePath);
        var combined = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);

        // Extract key (32 bytes for AES-256) and IV (16 bytes)
        var key = new byte[32];
        var iv = new byte[16];
        Buffer.BlockCopy(combined, 0, key, 0, 32);
        Buffer.BlockCopy(combined, 32, iv, 0, 16);

        return (key, iv);
    }
}