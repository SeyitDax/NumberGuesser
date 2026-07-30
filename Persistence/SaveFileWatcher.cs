namespace NumberGuesser.Persistence;
using NumberGuesser.Core;
using NumberGuesser.Core.Events;

public class SaveFileWatcher : IDisposable
{
	private readonly FileSystemWatcher _fileWatcher;

	public SaveFileWatcher(string directory, string fileName, EventBus eventBus)
	{
		_fileWatcher = new(directory, fileName);
		_fileWatcher.Deleted += (sender, e) => 
		{
			eventBus.Publish(new SaveFileDeletedEvent(e.FullPath));
		};
		_fileWatcher.EnableRaisingEvents = true;
	}

	public void Dispose() => _fileWatcher.Dispose();
}

