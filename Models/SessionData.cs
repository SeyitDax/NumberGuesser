namespace NumberGuesser.Models;

public record SessionData(
	DateTime LaunchTime,
	TimeSpan TotalDuration,
	int RunCount,
	bool SaveFileDeleted,
	bool Restarted
);
