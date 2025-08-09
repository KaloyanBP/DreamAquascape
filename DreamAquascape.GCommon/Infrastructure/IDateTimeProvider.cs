namespace DreamAquascape.GCommon.Infrastructure
{
    /// <summary>
    /// Abstraction for DateTime operations to enable testability
    /// </summary>
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
