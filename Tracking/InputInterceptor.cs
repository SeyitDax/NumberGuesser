namespace NumberGuesser.Tracking;

using NumberGuesser.Core;
using NumberGuesser.Models;
using System.Text;

public class InputInterceptor
{
	private readonly ConsoleRenderer _renderer;
	public InputInterceptor(ConsoleRenderer renderer)
	{
		_renderer = renderer;
	}

	public InputSnapshot ReadInput()
	{
		TimeSpan elapsedTime = TimeSpan.Zero;

		var phrase = new StringBuilder();
		var startTime = DateTime.Now;

		var backspaceCount = 0;

		var firstKey = true;
		var triedBlank = false;
		var deletedEverything = false;
		var wasFullRewrite = false;
		var finished = false;
		
		while (!finished)
		{
			ConsoleKeyInfo key = Console.ReadKey(intercept:true);
		
			if (key.Key == ConsoleKey.Enter)
			{
				if (phrase.Length == 0) triedBlank = true;
				else if (deletedEverything) wasFullRewrite = true;
				finished = true;
			}
			else if (key.Key == ConsoleKey.Backspace)
			{
				backspaceCount++;
				if (phrase.Length > 0) 
				{
					_renderer.Write("\b \b");
					phrase.Remove(phrase.Length - 1, 1);
				}
				if (phrase.Length == 0) deletedEverything = true;
			}
			else
			{
				if (firstKey)
				{
					elapsedTime = DateTime.Now - startTime;
					firstKey = false;
				}
				
				_renderer.Write(key.KeyChar);
				phrase.Append(key.KeyChar);
			}
		}

		return new InputSnapshot
		(
		 	Timestamp: DateTime.Now,
			RawText: phrase.ToString(),
			BackspaceCount: backspaceCount,
			DeletedEverything: deletedEverything,
			WasFullRewrite: wasFullRewrite,
			TriedBlankSubmission: triedBlank,
			HesitationMs: elapsedTime.TotalMilliseconds,
			WasInvalid: false
		);
	}
}
