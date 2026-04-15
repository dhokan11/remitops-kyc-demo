namespace RemitOps.API.Services
{
    public interface IAuditService
    {
        Task LogAsync(
            string actorUserId,
            string actorEmail,
            string actionType,
            string entityType,
            string? entityId,
            object? payload = null);
    }
}