using NumberGuesser.Core;
using NumberGuesser.Models;
using NumberGuesser.Tracking;

public class Program
{
	static int Main(string[] args)
	{
		var renderer = new ConsoleRenderer();
		var inputInterceptor = new InputInterceptor(renderer);
		
		InputSnapshot snapshot = inputInterceptor.ReadInput();

		Console.WriteLine();
		Console.WriteLine(snapshot);

		return 0;
	}
} 
