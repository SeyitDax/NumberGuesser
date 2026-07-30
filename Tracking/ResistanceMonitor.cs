namespace NumberGuesser.Tracking;

using System.Runtime.InteropServices;
using NumberGuesser.Core;
using NumberGuesser.Core.Events;

public class ResistanceMonitor : IDisposable
{
	private bool _started;
	private bool _disposed;

	private int _ctrlCAttempts;
	private bool _monitoring;

	private int _lostFocusCount;
	private bool _lostFocus;

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern IntPtr GetConsoleWindow();	

	private CancellationTokenSource _cts;
	private CancellationToken _token;
	private Task _task;

	public ResistanceMonitor(EventBus eventBus)
	{
		_cts = new();
		_task = Task.CompletedTask;
		eventBus.Subscribe<CtrlCPressedEvent>( e => _ctrlCAttempts++ );
	}

	public void Start()	
	{
		if (!_started && !_disposed)
		{
			_started = true;
			_monitoring = true;

			_token = _cts.Token;

			_task = Task.Run(async () =>
			{
				try
				{
					while (_monitoring)
					{
						bool isFocused = GetForegroundWindow() == GetConsoleWindow();
					
						if (!isFocused && !_lostFocus)
						{
							_lostFocusCount++;
							_lostFocus = true;			
						}
						else if (isFocused && _lostFocus)
						{
							_lostFocus = false;
						}

						await Task.Delay(500, _token);
					}
				}
				catch (OperationCanceledException)
				{}
			});
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;

			_monitoring = false;
			_cts.Cancel();
			_task.Wait();
			_cts.Dispose();
		}
	}
}
