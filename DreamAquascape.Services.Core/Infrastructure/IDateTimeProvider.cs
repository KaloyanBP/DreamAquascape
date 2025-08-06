namespace DreamAquascape.Services.Core.Infrastructure
{
    /// <summary>
    /// Abstraction for DateTime operations to enable testability
    /// </summary>
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
