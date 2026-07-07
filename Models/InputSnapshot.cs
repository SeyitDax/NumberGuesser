namespace NumberGuesser.Models;

public record InputSnapshot(
	DateTime Timestamp, 
	string RawText, 
	int BackspaceCount,
	bool WasFullRewrite,
	double HesitationMs,
	bool WasInvalid
);

