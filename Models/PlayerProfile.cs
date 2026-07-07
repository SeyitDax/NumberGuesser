namespace NumberGuesser.Models;

public record PlayerProfile( string Name, string Email)
{
	public TraitVector Traits { get; init; } = new();
	public SessionData Session { get; init; } = new SessionData(
		DateTime.Now,
		TimeSpan.Zero,
		0,
		false,
		false
	);
	
	public List<InputSnapshot> InputHistory { get; init; } = new(); 
	public List<DilemmaRecord> DilemmaHistory { get; init; } = new();
	public List<OsintPayload> OsintHistory { get; init; } = new();
}
	
