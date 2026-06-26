using AirplaneSystem.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace AirplaneSystem.Infrastructure.Security;

public class AesEncryptionService : IEncryptionService
{
    private const string EncPrefix = "ENC:";
    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration config)
    {
        var masterKey = config["AIRSYSTEM_MASTER_KEY"]
            ?? Environment.GetEnvironmentVariable("AIRSYSTEM_MASTER_KEY")
            ?? "default-dev-key-change-in-production-123";

        using var pbkdf2 = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(masterKey),
            Encoding.UTF8.GetBytes("AirSystemSalt2024"),
            100_000,
            HashAlgorithmName.SHA256);
        _key = pbkdf2.GetBytes(32);
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
            sw.Write(plainText);

        return EncPrefix + Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (!IsEncrypted(cipherText)) return cipherText;

        var data = Convert.FromBase64String(cipherText[EncPrefix.Length..]);
        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[aes.BlockSize / 8];
        Array.Copy(data, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var ms = new MemoryStream(data, iv.Length, data.Length - iv.Length);
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }

    public bool IsEncrypted(string value) =>
        !string.IsNullOrEmpty(value) && value.StartsWith(EncPrefix);
}
