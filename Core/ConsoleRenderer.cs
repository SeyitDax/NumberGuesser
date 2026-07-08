namespace NumberGuesser.Core;

public class ConsoleRenderer
{
	public void Write(string text, ConsoleColor color = ConsoleColor.White)
	{
		Console.ForegroundColor = color;
		Console.Write(text);
		Console.ResetColor();
	}

	public void Write(char character, ConsoleColor color = ConsoleColor.White)
	{
		Console.ForegroundColor = color;
		Console.Write(character);
		Console.ResetColor();
	}

	public void WriteLine(string text, ConsoleColor color = ConsoleColor.White)
	{
		Console.ForegroundColor = color;
		Console.WriteLine(text);
		Console.ResetColor();
	}

	public void WriteLine(char character, ConsoleColor color = ConsoleColor.White)
	{
		Console.ForegroundColor = color;
		Console.WriteLine(character);
		Console.ResetColor();
	}

	public void Clear()
	{
		Console.Clear();
	}

	public string ReadLine()
	{
		var result = Console.ReadLine();
		return result!;
	}
}
