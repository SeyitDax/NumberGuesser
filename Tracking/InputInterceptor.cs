namespace NumberGuesser.Core;
using NumberGuesser.Models;
using System.Text;

public class InputInterceptor
{
	private readonly ConsoleRenderer _renderer;
	public InputInterceptor(ConsoleRenderer Renderer)
	{
		_renderer = Renderer;
	}

	public InputSnapshot ReadInput()
	{
		TimeSpan elapsedTime = TimeSpan.Zero;

		var phrase = new StringBuilder();
		var startTime = DateTime.Now;

		var backspaceCount = 0;

		var firtKey = true;
		var triedBlank = false;
		var wasFullRewrite = false;
		var finished = false;
		
		while (!finished)
		{
			var deleteCount = 0;
			ConsoleKeyInfo key = Console.ReadKey(intercept:true);
		
			if (key.Key == ConsoleKey.Enter)
			{
				if (phrase?.Length == 0) triedBlank = true;
				finished = true;
			}
			else if (key.Key == ConsoleKey.Backspace)
			{
				deleteCount++;
				backspaceCount++;
				if (phrase != null)
				{
					if (deleteCount > phrase.Length) { }
					else if (phrase.Length >= deleteCount) 
					{
						_renderer.Write("\b \b");
						phrase.Remove(phrase.Length - 1, 1);
					}
					if (phrase.Length == deleteCount) wasFullRewrite = true;
				}
				deleteCount--;
			}
			else
			{
				if (firtKey) elapsedTime = DateTime.Now - startTime;
				
				_renderer.Write(key.KeyChar);
				phrase?.Append(key.KeyChar);
			}
		}

		return new InputSnapshot
		(
		 	Timestamp: DateTime.Now,
			RawText: phrase.ToString(),
			BackspaceCount: backspaceCount,
			WasFullRewrite: wasFullRewrite,
			TriedBlankSubmission: triedBlank,
			HesitationMs: elapsedTime.TotalMilliseconds,
			WasInvalid: false
		);
	}
}
