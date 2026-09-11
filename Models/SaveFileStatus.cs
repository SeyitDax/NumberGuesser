namespace NumberGuesser.Models;

public record SaveFileStatus(
	DateTime Timestamp,
	bool CreatedOnce,
	int RunCount,
	FileStatus FileStatus
);
