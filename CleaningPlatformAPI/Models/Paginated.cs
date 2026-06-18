namespace CleaningPlatformAPI.Models;

public record Paginated<T>(List<T> Data, int Total);
