using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace NumberGuesser.Core;

public class EventBus
{
	private readonly Dictionary<Type, List<Delegate>> _subscribers = new();	

	public void Subscribe<T>(Action<T> handler)
	{
		Type type = typeof(T);

		ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(
				_subscribers, type, out bool exists);
		if (!exists)
		{
			list = new();
		}
		list!.Add(handler);
	}

	public void Publish<T>(T eventData)
	{
		Type type = typeof(T);

		ref var list = ref CollectionsMarshal.GetValueRefOrNullRef(
				_subscribers, type);

		if(Unsafe.IsNullRef(ref list))
		{
			return; // No list is found
		}

		foreach (var handler in list)
		{
			((Action<T>)handler).Invoke(eventData);
		}
	}
}
