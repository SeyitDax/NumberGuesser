namespace NumberGuesser.Persistence;

using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using NumberGuesser.Models;

public class SessionStore
{
	private readonly string _storagePath;
	private readonly string _password;

	private readonly EncryptedFileManager _encryptedFileManager;
	private bool newPlayer;

	public SessionStore()
	{
		_storagePath = GetSessionPath();
		_password = DerivePassword();
		_encryptedFileManager = new EncryptedFileManager();
	}

	public void SavePlayerProfile(PlayerProfile playerProfile)
	{
		var jsonProfile = JsonSerializer.Serialize(playerProfile);	
		_encryptedFileManager.EncryptToFile(_storagePath, jsonProfile, _password);
	}
	
	public PlayerProfile LoadPlayerProfile()
	{
		var jsonProfile = String.Empty;
		try
		{
			jsonProfile = _encryptedFileManager.DecryptFromFile(_storagePath, _password);
		}
		catch (Exception ex)
		{
			newPlayer = true;
		}

		if (!String.IsNullOrEmpty(jsonProfile))
		{
			var playerProfile = JsonSerializer.Deserialize<PlayerProfile>(jsonProfile);
			if(playerProfile != null)
				return playerProfile;
		}

		return new PlayerProfile(Environment.UserName);
	}
	
	private string GetSessionPath()
	{
		var numberGuesserDataDir = Path.Combine(Environment.GetFolderPath(
					Environment.SpecialFolder.LocalApplicationData),
					"NumberGuesser");
		Directory.CreateDirectory(numberGuesserDataDir);

		var seedTag = "F2hf9T1Ksr*^@qp22bH9@5KlN&o9Er";
	
		var tagBytes = Encoding.UTF8.GetBytes(seedTag);
		var hashedBytes = SHA256.HashData(tagBytes);

		var hexSeed = Convert.ToHexString(hashedBytes);
		var seed16 = hexSeed[..16];

		// Creating the path using the generated seed
		var path = Path.Combine(numberGuesserDataDir, seed16 + ".dat");

return path;
	}

	// TODO(release): obscurity-grade, not real protection — MachineName/UserName aren't secret
	// and this derivation is public in source, so anyone with the repo can recompute the password                   
	// on their own machine. Fine for the portfolio version; before a real release, replace with                     
	// key material a technical user can't just recompute (e.g. Windows DPAPI /                                      
	 // System.Security.Cryptography.ProtectedData, tied to the actual logon credential).         
	private string DerivePassword()
	{
		var seedTag = Environment.MachineName + "|" + Environment.UserName;

		var tagBytes = Encoding.UTF8.GetBytes(seedTag);
		var hashedBytes = SHA256.HashData(tagBytes);

		var hexSeed = Convert.ToHexString(hashedBytes);

		return hexSeed;
	}


}
