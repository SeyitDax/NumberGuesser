namespace NumberGuesser.Models;

public record DilemmaRecord(
	string DilemmaId,
	string ChoiceMade,
	bool WasDecoy,
	DateTime Timestamp,
	Dictionary<TraitDimension, double> Delta
);
