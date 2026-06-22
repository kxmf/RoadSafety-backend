namespace RoadSafety_backend.Infrastructure.Services.Push;

public sealed class FcmOptions
{
    public bool Enabled { get; set; }
    public string? ProjectId { get; set; }
    public string? ServiceAccountJsonPath { get; set; }
    public string? ServiceAccountJson { get; set; }
}
