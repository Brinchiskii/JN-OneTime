namespace OneTime.Api.Models.AuditLogsDto
{
	public record AuditLogDto(int AuditLogId,DateTime Timestamp,string Action,string EntityType,int? EntityId,int? ActorUserId,string? ActorUserName,string? Details);
}
