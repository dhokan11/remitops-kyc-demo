using Dapper;
using System.Data;
using System.Text.Json;
using RemitOps.API.Data;

namespace RemitOps.API.Services
{
    public class AuditService : IAuditService
    {
        private readonly IDbConnectionFactory _factory;
        private readonly IHttpContextAccessor _http;

        public AuditService(IDbConnectionFactory factory, IHttpContextAccessor http)
        {
            _factory = factory;
            _http = http;
        }

        public async Task LogAsync(
            string actorUserId,
            string actorEmail,
            string actionType,
            string entityType,
            string? entityId,
            object? payload = null)
        {
            var ctx = _http.HttpContext;
            var ip = ctx?.Connection?.RemoteIpAddress?.ToString();
            var ua = ctx?.Request?.Headers["User-Agent"].ToString();
            var payloadJson = payload == null ? null : JsonSerializer.Serialize(payload);

            using var db = _factory.CreateConnection();
            await db.ExecuteAsync(
                "sp_AuditLog_Insert",
                new
                {
                    ActorUserId = actorUserId,
                    ActorEmail = actorEmail,
                    ActionType = actionType,
                    EntityType = entityType,
                    EntityId = entityId,
                    PayloadJson = payloadJson,
                    IpAddress = ip,
                    UserAgent = ua
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}