namespace NumberGuesser.Models;

public record TraitVector()
{
	public Dictionary<TraitDimension, double> Scores { get; init; } = 
		Enum.GetValues<TraitDimension>().
		ToDictionary(dimension => dimension, dimension => 0.5); 
}


