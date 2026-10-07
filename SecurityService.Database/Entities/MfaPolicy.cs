namespace SecurityService.Database.Entities;

public enum MfaPolicyTargetType
{
    User = 1,
    Role = 2
}

public sealed class MfaPolicy
{
    public Guid Id { get; set; }

    public MfaPolicyTargetType TargetType { get; set; }

    public string TargetId { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }
}
