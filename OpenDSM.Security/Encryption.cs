using ChaseLabs.CLConfiguration;
using System.Security.Cryptography;
using System.Text;

namespace OpenDSM.Security;

public class Encryption
{
    private readonly string salt;
    public static Encryption Instance = Instance ??= new();
    private Encryption()
    {
        // Gets or Generates Security KEY
        ConfigManager manager = new("sec", OpenDSM.Core.Global.RootDirectory);
        salt = manager.GetOrCreate("enc_key", Guid.NewGuid().ToString().Replace("-", "")).Value;
    }
    /// <summary>
    /// Encrypts text using AES
    /// </summary>
    /// <param name="plainText">The text to be encrypted</param>
    /// <returns>Encrypted Text</returns>
    /// <exception cref="ArgumentNullException">If the text is empty or null</exception>
    public string EncryptString(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText)) throw new ArgumentNullException("plainText", "The plainText cannot be null or empty");

        byte[] secret = Encoding.ASCII.GetBytes(salt);
        using Aes aes = Aes.Create();
        ICryptoTransform transform = aes.CreateEncryptor(secret, secret);
        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, transform, CryptoStreamMode.Write);
        using (StreamWriter writer = new(cs))
        {
            writer.Write(plainText);
        }
        byte[] bytes = ms.ToArray();
        return Convert.ToBase64String(bytes).Replace("==", "");
    }

    /// <summary>
    /// Decrypts string to plainText using AES
    /// </summary>
    /// <param name="cryptText">The encrypted text</param>
    /// <returns>The plainText</returns>
    /// <exception cref="ArgumentNullException">If the encrypted text is empty or null</exception>
    public string DecrptString(string cryptText)
    {
        if (string.IsNullOrWhiteSpace(cryptText)) throw new ArgumentNullException("cryptText", "The cryptText cannot be null or empty");
        
        string base64 = $"{cryptText}==";
        if (!Core.Handlers.FileHandler.IsValidBase64(base64)) throw new ArgumentNullException("cryptText", "The cryptText is not valid!");

        byte[] secret = Encoding.ASCII.GetBytes(salt);
        using Aes aes = Aes.Create();
        ICryptoTransform transform = aes.CreateDecryptor(secret, secret);
        using MemoryStream ms = new(Convert.FromBase64String(base64));
        using CryptoStream cs = new(ms, transform, CryptoStreamMode.Read);
        using StreamReader reader = new(cs);
        return reader.ReadToEnd();
    }
}