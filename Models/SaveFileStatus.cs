namespace NumberGuesser.Models;

public record SaveFileStatus(
	DateTime Timestamp,
	bool Readable,
	bool CreatedOnce,
	int RunCount,
	FileStatus FileStatus
);
