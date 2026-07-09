namespace NumberGuesser.Persistence;

using System.Text;
using System.Security.Cryptography;

public class EncryptedFileManager
{
	public void EncryptToFile(string filePath, string plainText, string password)
	{
		byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
		byte[] salt = RandomNumberGenerator.GetBytes(16);

		using var derivedKey = new Rfc2898DeriveBytes(
				password: passwordBytes,
				salt: salt,
				iterations: 100_000,
				hashAlgorithm: HashAlgorithmName.SHA256
				);
		
		byte[] key = derivedKey.GetBytes(32); // 256 bits
		byte[] iv = RandomNumberGenerator.GetBytes(16); // 128 bits
		
		using var aes = Aes.Create();
		aes.Key = key;

		byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
		byte[] cipherText = aes.EncryptCbc(plainTextBytes, iv);

		using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
		fs.Write(salt);
		fs.Write(iv);
		fs.Write(cipherText);
	}

	public string DecryptFromFile(string filePath, string password)
	{
		byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

		using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		byte[] salt = new byte[16];
		byte[] iv = new byte[16];
		fs.ReadExactly(salt, 0, 16);
		fs.ReadExactly(iv, 0, 16);

		byte[] cipherText = new byte[(fs.Length - fs.Position)];
		fs.ReadExactly(cipherText);
	
		using var derivedKey = new Rfc2898DeriveBytes(
				password: passwordBytes,
				salt: salt,
				iterations: 100_000,
				hashAlgorithm: HashAlgorithmName.SHA256
				);
		
		byte[] key = derivedKey.GetBytes(32);

		using var aes = Aes.Create();
		aes.Key = key;

		byte[] plainTextBytes = aes.DecryptCbc(cipherText, iv);
		string plainText = Encoding.UTF8.GetString(plainTextBytes);

		return plainText;
	}
}
