namespace NumberGuesser.Models;

public record OsintPayload (
	OsintSource Source,
	string Snippet,
	double Confidence,
	DateTime Timestamp
);
