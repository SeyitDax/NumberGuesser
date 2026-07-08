namespace NumberGuesser.Core;

public class TypewriterEffect(ConsoleRenderer renderer)
{
   ConsoleRenderer _renderer = renderer; 

   public async Task TypeAsync(string text, int delayMs = 30, ConsoleColor color = ConsoleColor.White)
   {
	   _renderer.Write("\n");
	   foreach (var character in text)
	   {
		_renderer.Write(character, color);
		await Task.Delay(delayMs);
	   }
   }
}
