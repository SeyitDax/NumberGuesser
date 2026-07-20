namespace NumberGuesser.Models;

public record InputSnapshot(
	DateTime Timestamp, 
	string RawText, 
	int BackspaceCount,
	bool DeletedEverything,
	bool WasFullRewrite,
	bool TriedBlankSubmission,
	double HesitationMs,
	bool WasInvalid
);

