using NumberGuesser.Core;
using NumberGuesser.Models;
using NumberGuesser.Tracking;

public class Program
{
	static int Main(string[] args)
	{
		var eventBus = new EventBus();
		var renderer = new ConsoleRenderer();
		var inputInterceptor = new InputInterceptor(renderer);

		var resistanceMonitor = new ResistanceMonitor(eventBus);
		var rapidPressEsclation = new RapidPressEscalation(eventBus);
		
		resistanceMonitor.Start();
		rapidPressEsclation.Start();

		renderer.WriteLine("TYPE: ");

		InputSnapshot snapshot = inputInterceptor.ReadInput();

		renderer.WriteLine("Input Snapshot");
		renderer.WriteLine("\n");
		renderer.WriteLine(snapshot.ToString());

		renderer.WriteLine("Resistance Snapshot");
		renderer.WriteLine("\n");
		renderer.WriteLine(resistanceMonitor.GetResistanceSnapshot().ToString());
		return 0;
	}
} 
