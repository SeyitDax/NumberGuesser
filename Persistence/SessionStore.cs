namespace NumberGuesser.Persistence;

using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

using NumberGuesser.Models;
using NumberGuesser.Core;
using NumberGuesser.Core.Events;

public class SessionStore
{
	private readonly string _storagePath;
	private readonly string _saveFileStatusPath;
	private readonly string _password;

	private readonly EncryptedFileManager _encryptedFileManager;
	private readonly EventBus _eventBus;

	private List<string> _errorsEncountered;
	private SaveFileStatus _saveFileStatus;
	public bool NewPlayer { get; private set; }

	public SessionStore(List<string> errorsEncountered, EventBus eventBus)
	{
		_storagePath = GetSessionPath();
		_saveFileStatusPath = GetSaveFileStatusPath();
		_eventBus = eventBus;
		_password = DerivePassword();
		_encryptedFileManager = new EncryptedFileManager();

		_errorsEncountered = errorsEncountered;

		_saveFileStatus = LoadSaveFileStatus();
	}

	public void SavePlayerProfile(PlayerProfile playerProfile)
	{
		var jsonProfile = JsonSerializer.Serialize(playerProfile);	
		_encryptedFileManager.EncryptToFile(_storagePath, jsonProfile, _password);
	}
	
	public PlayerProfile LoadPlayerProfile()
	{
		try
		{
			var jsonProfile = _encryptedFileManager.DecryptFromFile(_storagePath, _password);
			var playerProfile = JsonSerializer.Deserialize<PlayerProfile>(jsonProfile);
			if(playerProfile != null)
				return playerProfile;
		}
		catch (FileNotFoundException ex)
		{
			_errorsEncountered.Add(ex.Message);
			if (!_saveFileStatus.CreatedOnce)
			{
				NewPlayer = true;
				return new PlayerProfile(Environment.UserName);
			}

			_eventBus.Publish(new SaveFileDeletedEvent(_storagePath));
			return new PlayerProfile(Environment.UserName, FallbackProfile: true);
		}
		catch (IOException ex)
		{
			_errorsEncountered.Add(ex.Message);
			return new PlayerProfile(Environment.UserName, FallbackProfile: true);
			
		}
		catch (CryptographicException ex)
		{
			_errorsEncountered.Add(ex.Message);
			return new PlayerProfile(Environment.UserName, FallbackProfile: true);
		}

		return new PlayerProfile(Environment.UserName);
	}

	public void SaveSaveFileStatus(SaveFileStatus status)
	{
		var jsonStatus = JsonSerializer.Serialize(status);
		_encryptedFileManager.EncryptToFile(_saveFileStatusPath, jsonStatus, _password);
	}

	public SaveFileStatus LoadSaveFileStatus()
	{
		try
		{
			var jsonStatus = _encryptedFileManager.DecryptFromFile(_saveFileStatusPath, _password);
			var saveFileStatus = JsonSerializer.Deserialize<SaveFileStatus>(jsonStatus);
			if(saveFileStatus != null)
				return saveFileStatus;
		}
		catch (FileNotFoundException ex) 
		{
			_errorsEncountered.Add(ex.Message);

			return new SaveFileStatus(
				Timestamp:DateTime.Now,
				Readable: true,
				CreatedOnce:false,
				RunCount:0,
				FileStatus.NeverHadSave);
		}
		catch (IOException ex)
		{
			_errorsEncountered.Add(ex.Message);
			
			return new SaveFileStatus(
				Timestamp:DateTime.Now,
				Readable: false,
				CreatedOnce:false,
				RunCount:0,
				FileStatus.NeverHadSave);
		}
		catch (CryptographicException ex)
		{
			_errorsEncountered.Add(ex.Message);
			
			return new SaveFileStatus(
				Timestamp:DateTime.Now,
				Readable: false,
				CreatedOnce:false,
				RunCount:0,
				FileStatus.NeverHadSave);
		}

		return new SaveFileStatus(
				Timestamp:DateTime.Now,
				Readable: true,
				CreatedOnce:true,
				RunCount:0,
				FileStatus.SaveExist);

	}
	
	private string GetSessionPath()
	{
		var saveFileDataDir = Path.Combine(Environment.GetFolderPath(
					Environment.SpecialFolder.LocalApplicationData),
					"ng");
		Directory.CreateDirectory(saveFileDataDir);

		var seedTag = "F2hf9T1Ksr*^@qp22bH9@5KlN&o9Er";
	
		var tagBytes = Encoding.UTF8.GetBytes(seedTag);
		var hashedBytes = SHA256.HashData(tagBytes);

		var hexSeed = Convert.ToHexString(hashedBytes);
		var seed16 = hexSeed[..16];

		// Creating the path using the generated seed
		var path = Path.Combine(saveFileDataDir, seed16 + ".dat");

		return path;
	}

	private string GetSaveFileStatusPath()
	{
		var statusDataDir = Path.Combine(Environment.GetFolderPath(
					Environment.SpecialFolder.LocalApplicationData),
					"ngsfs");
		Directory.CreateDirectory(statusDataDir);
	
		var seedTag = "F2hf9T1Ksr*^@qp22bH9@5KlN&o9Er";
	
		var tagBytes = Encoding.UTF8.GetBytes(seedTag);
		var hashedBytes = SHA256.HashData(tagBytes);

		var hexSeed = Convert.ToHexString(hashedBytes);
		var seed16 = hexSeed[..16];

		var path = Path.Combine(statusDataDir, seed16 + ".dat");
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
