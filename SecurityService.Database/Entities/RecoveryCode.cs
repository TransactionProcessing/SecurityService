namespace SecurityService.Database.Entities;

public sealed class RecoveryCode
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string CodeHash { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }

    public DateTime? ConsumedUtc { get; set; }
}
