using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FCInvoiceUI.Services;

public class EncryptionService
{
    private readonly string _keyFilePath;

    public EncryptionService()
    {
        _keyFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Data", "KeyIV.dat");

        if (!File.Exists(_keyFilePath))
        {
            var (key, iv) = GenerateKeyAndIV();
            SaveKeyAndIV(key, iv, _keyFilePath);
        }
    }

    public void EncryptFile(string inputFile, string outputFile)
    {
        var (key, iv) = LoadKeyAndIV();

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using FileStream fsInput = new(inputFile, FileMode.Open, FileAccess.Read);
        using FileStream fsEncrypted = new(outputFile, FileMode.Create, FileAccess.Write);
        using CryptoStream cs = new(fsEncrypted, aes.CreateEncryptor(), CryptoStreamMode.Write);

        fsInput.CopyTo(cs);
    }

    public void DecryptFile(string inputFile, string outputFile)
    {
        var (key, iv) = LoadKeyAndIV();

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using FileStream fsEncrypted = new(inputFile, FileMode.Open, FileAccess.Read);
        using FileStream fsDecrypted = new(outputFile, FileMode.Create, FileAccess.Write);
        using CryptoStream cs = new(fsEncrypted, aes.CreateDecryptor(), CryptoStreamMode.Read);

        cs.CopyTo(fsDecrypted);
    }

    public string Encrypt(string plainText)
    {
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

    public string Decrypt(string cipherText)
    {
        try
        {
            var (key, iv) = LoadKeyAndIV();
            byte[] buffer = Convert.FromBase64String(cipherText);

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

    private static (byte[] Key, byte[] IV) GenerateKeyAndIV()
    {
        using Aes aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();
        return (aes.Key, aes.IV);
    }

    private static void SaveKeyAndIV(byte[] key, byte[] iv, string filePath)
    {
        byte[] combined = new byte[key.Length + iv.Length];
        Buffer.BlockCopy(key, 0, combined, 0, key.Length);
        Buffer.BlockCopy(iv, 0, combined, key.Length, iv.Length);

        byte[] encrypted = ProtectedData.Protect(combined, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(filePath, encrypted);
    }

    private (byte[] Key, byte[] IV) LoadKeyAndIV()
    {
        byte[] encrypted = File.ReadAllBytes(_keyFilePath);
        byte[] combined = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);

        byte[] key = new byte[32];
        byte[] iv = new byte[16];
        Buffer.BlockCopy(combined, 0, key, 0, 32);
        Buffer.BlockCopy(combined, 32, iv, 0, 16);

        return (key, iv);
    }
}
