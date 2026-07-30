namespace NumberGuesser.Tracking;

using NumberGuesser.Core;
using NumberGuesser.Core.Events;

public class RapidPressEscalation : IDisposable
{
	private Queue<DateTime> _rapidPresses;
	private ConsoleCancelEventHandler _cancelEventHandler;
	private EventBus _eventBus;
	private const float WindowSeconds = 0.5f;

	private bool _started;
	private bool _disposed;

	public RapidPressEscalation(EventBus eventBus)
	{
		_rapidPresses = new();
		_cancelEventHandler = (sender, e) => { };
		_eventBus = eventBus;
	}

	public void Start()
	{
		if (!_started)
		{
			_started = true;
			_cancelEventHandler = (sender, e) =>
			{
				e.Cancel = true;
	
				_rapidPresses.Enqueue(DateTime.Now);
				
				while (_rapidPresses.Count > 0 && (DateTime.Now - _rapidPresses.Peek()).TotalSeconds > WindowSeconds)
				{
					_rapidPresses.Dequeue();
				}
				_eventBus.Publish(new CtrlCPressedEvent(_rapidPresses.Count));
	
				if (_rapidPresses.Count >= 3)
				{
					_rapidPresses.Clear();
					_eventBus.Publish(new CtrlCEscalationEvent());
					Environment.Exit(0);
				}
			};
			Console.CancelKeyPress += _cancelEventHandler;
		}
	}

	public void Dispose() 
	{
		if (_started && !_disposed)
		{
			_disposed = true;
			Console.CancelKeyPress -= _cancelEventHandler;
		}
	}
}

